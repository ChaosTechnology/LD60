using System;
using System.Linq;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosFramework.Menu.OpenGl.Controls.Layout;
using ChaosUtil.Reflection;
using LD60.World.Tactician;
using OpenTK.Graphics.OpenGL;
using static ChaosFramework.Graphics.OpenGl.Graphics;
using SysCol = System.Collections.Generic;

namespace LD60.World
{
    using PickSample = (Vector3f pos, Vector3f nor, int pick);

    public class Overlay
        : DialogScene
    {
        readonly FloatingIslands islands;
        readonly SysCol.Dictionary<int, Pickable> registeredPickables = [];
#if DEBUG
        readonly TextureContainer.Entry pickingPos, pickingNor;
#endif
        readonly Framebuffer pickingFbo;
        readonly Bounds2i viewPort;
        readonly float ratio;

        int nextPickingIndex = 0;
        bool hasPickingData = false;

        public Pickable picked {get; private set;} = null;
        public Vector3f worldPos {get; private set;} = Vector3f.NAN;

        public readonly FlowPanel subTools;
        readonly FlowPanel tools;
        readonly Label gold;

        public Tool currentTool {get; private set;}

        public Overlay(FloatingIslands islands)
            : base(islands.game.menuContext)
        {
            this.islands = islands;
            panel.textureSource = string.Empty;

            ratio = islands.view.screenRatio;
            viewPort = new(0, 0, islands.deferredShader.width, islands.deferredShader.height);

            Texture.Parameters texParams = new(
                viewPort.width,
                viewPort.height,
                PixelType.Float,
                TextureMinFilter.Nearest,
                TextureMagFilter.Nearest,
                PixelFormat.Rgba,
                PixelInternalFormat.Rgba32f
                );

            Texture pos = new(islands.game.graphics.dispatcher, texParams);
            Texture nor = new(islands.game.graphics.dispatcher, texParams);
#if DEBUG
            pickingPos = TextureContainer.Entry.Mock((_, _) => pos, _ => pos.Dispose());
            pickingNor = TextureContainer.Entry.Mock((_, _) => nor, _ => nor.Dispose());
#endif
            pickingFbo = new Framebuffer(viewPort, [pos, nor], islands.deferredShader.depthBuffer);

            BuildMenu("overlay", "Root/Overlay", null, null);

#if DEBUG
            Vector2f v = new(ratio, 1);
            panel.CreateControl<Image>(v * 0.8f, v * 0.15f).texture = pickingPos;
            panel.CreateControl<Image>(v * 0.8f - new Vector2f(0, v.y  * 0.32f), v * 0.15f).texture = pickingNor;
#endif

            tools = panel.GetControlById("Tools") as FlowPanel;
            foreach(Type toolType in AssemblyManager.SubTypesOf<Tool>())
                if (!toolType.IsAbstract)
                {
                    Tool tool = (Tool)islands.AddComponent(toolType);
                    Button btn = tools.CreateControl<Button>(0, 0.05f, toolType.Name);

                    bool Enabled()
                        => tool.Allowed() && islands.overlay.currentTool != tool;

                    void Select(Control _)
                    {
                        islands.overlay.currentTool = tool;
                        tool.Select();
                    }

                    btn.enabledFunction = Enabled;
                    btn.mouseDownLeft += Select;
                }

            subTools = panel.GetControlById("Placables") as FlowPanel;
            gold = panel.GetControlById("Coins") as Label;
        }

        public void SelectDefaultTool()
        {
            Button startupTool = tools.EnumerateChildren<Button>(false).First();
            startupTool.mouseDownLeft(startupTool);
            if (!currentTool.Allowed())
                throw new InvalidOperationException("Default tool selection should select tool by type, not order.");
        }

        public void BeginPicking()
        {
            hasPickingData = true;
            islands.game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, pickingFbo);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            updateLayers[(int)UpdateLayers.HudActions].Add(PickOnGoblins);
            updateLayers[(int)UpdateLayers.HudActions].Add(UpdateGui);
        }

        void PickOnGoblins()
        {
            Vector2i pixel = CursorToPixel();
            PickSample sample = SamplePickable(pixel);

            if (!registeredPickables.TryGetValue(sample.pick, out Pickable found))
            {
                System.Diagnostics.Debug.WriteLine("Dead pickable got picked!");
                sample.pick = 0;
            }

            worldPos = sample.pick > 0 ? sample.pos : Vector3f.NAN;
            picked = sample.pick > 0 ? found : null;
        }

        void UpdateGui()
        {
            gold.text = islands.inventory.gold.ToString();
        }

        public void Register(Pickable p)
            => registeredPickables[p.pickingIndex = ++nextPickingIndex] = p;

        public void Unregister(Pickable p)
            => registeredPickables.Remove(p.pickingIndex);

        Vector2i CursorToPixel()
        {
            int x = (int)MathF.Floor((context.cursorPosition.x + ratio) / (2 * ratio) * viewPort.width);
            int y = (int)MathF.Floor((context.cursorPosition.y + 1) / 2 * viewPort.height);
            return new Vector2i(
                Math.Clamp(x, 0, viewPort.width - 1),
                Math.Clamp(y, 0, viewPort.height - 1)
                );
        }

        PickSample SamplePickable(Vector2i pixel)
        {
            if (!hasPickingData)
                return (Vector3f.NAN, Vector3f.NAN, 0);

            Framebuffer oldFb = context.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, pickingFbo);

            float[] pos = new float[4];
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            ThrowErrors();
            GL.ReadPixels(pixel.x, pixel.y, 1, 1, PixelFormat.Rgba, PixelType.Float, pos);
            ThrowErrors();

            float[] nor = new float[4];
            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            ThrowErrors();
            GL.ReadPixels(pixel.x, pixel.y, 1, 1, PixelFormat.Rgba, PixelType.Float, nor);
            ThrowErrors();

            context.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, oldFb);
            int pick = (int)Math.Round(pos[3]); // graphics drivers may produce rounding errors

            return (
                new Vector3f(pos[0], pos[1], pos[2]),
                new Vector3f(nor[0], nor[1], nor[2]),
                pick
                );
        }
    }
}
