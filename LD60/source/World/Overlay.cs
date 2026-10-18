using System;
using System.Diagnostics;
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
    public class Overlay
        : DialogScene
    {
        readonly FloatingIslands islands;
        readonly SysCol.Dictionary<int, Pickable> registeredPickables = [];
        readonly TextureContainer.Entry pickingTex;
        readonly Framebuffer pickingFbo;
        readonly Bounds2i viewPort;
        readonly float ratio;

        int nextPickingIndex = 0;
        bool hasPickingData = false;

        public Pickable picked {get; private set;} = null;
        public Vector3f worldPos {get; private set;} = Vector3f.NAN;

        public readonly FlowPanel subTools;
        readonly FlowPanel tools;

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
                PixelFormat.Red,
                PixelInternalFormat.R32f
                );

            Texture tex = new(islands.game.graphics.dispatcher, texParams);
            pickingTex = TextureContainer.Entry.Mock((_, _) => tex, _ => tex.Dispose());
            pickingFbo = new Framebuffer(viewPort, [tex], islands.deferredShader.depthBuffer);

            BuildMenu("overlay", "Root/Overlay", null, null);

#if DEBUG
            Vector2f v = new(ratio, 1);
            panel.CreateControl<Image>(v * 0.8f, v * 0.15f).texture = pickingTex;
#endif

            tools = panel.GetControlById("Tools") as FlowPanel;
            foreach(Type toolType in AssemblyManager.SubTypesOf<Tool>())
            {
                Tool tool = (Tool)islands.AddComponent(toolType);
                Button btn = tools.CreateControl<Button>(0, 0.05f, toolType.Name);

                bool Enabled()
                    => islands.overlay.currentTool != tool;

                void Select(Control _)
                {
                    islands.overlay.currentTool = tool;
                    tool.Select();
                }

                btn.enabledFunction = Enabled;
                btn.mouseDownLeft += Select;
            }

            subTools = panel.GetControlById("Placables") as FlowPanel;
        }

        public void SelectDefaultTool()
        {
            Button startupTool = tools.EnumerateChildren<Button>(false).First();
            startupTool.mouseDownLeft(startupTool);
        }

        public void BeginPicking()
        {
            hasPickingData = true;
            islands.game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, pickingFbo);
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            updateLayers[(int)UpdateLayers.HudActions].Add(PickOnGoblins);
        }

        void PickOnGoblins()
        {
            Vector2i pixel = CursorToPixel();
            int victim = SamplePickable(pixel);
            worldPos = victim > 0 ? SampleWorldPos(pixel) : Vector3f.NAN;
            picked = victim > 0 ? registeredPickables[victim] : null;
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

        int SamplePickable(Vector2i pixel)
        {
            if (!hasPickingData)
                return 0;

            Framebuffer oldFb = context.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, pickingFbo);

            float[] pickingIndex = new float[1];

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            ThrowErrors();

            GL.ReadPixels(pixel.x, pixel.y, 1, 1, PixelFormat.Red, PixelType.Float, pickingIndex);
            ThrowErrors();

            context.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, oldFb);

            int cast = (int)Math.Round(pickingIndex[0]); // graphics drivers may produce rounding errors
            return cast;
        }

        Vector3f SampleWorldPos(Vector2i pixel)
        {
            if (!hasPickingData)
                return 0;

            Framebuffer oldFb = context.graphics.stateTracker.BindFramebuffer(
                FramebufferTarget.Framebuffer,
                islands.deferredShader.worldFramebuffer
                );

            byte[] worldPos = new byte[16];

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            ThrowErrors();

            GL.ReadPixels(pixel.x, pixel.y, 1, 1, PixelFormat.Rgba, PixelType.Float, worldPos);
            ThrowErrors();

            context.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, oldFb);

            return new Vector3f(
                BitConverter.ToSingle(worldPos, 0),
                BitConverter.ToSingle(worldPos, 4),
                BitConverter.ToSingle(worldPos, 8)
                );
        }
    }
}
