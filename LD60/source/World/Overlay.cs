using System;
using System.Diagnostics;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using LD60.World.Enemies;
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

#if DEBUG
            Vector2f v = new(ratio, 1);
            panel.CreateControl<Image>(v * 0.8f, v * 0.15f).texture = pickingTex;
#endif
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
            int victim = SamplePickable();
            if (victim > 0)
                (registeredPickables[victim] as Goblin)?.Dispose();
        }

        public void Register(Pickable p)
            => registeredPickables[p.pickableIndex = ++nextPickingIndex] = p;

        public void Unregister(Pickable p)
            => registeredPickables.Remove(p.pickableIndex);

        Vector2i CursorToPixel()
        {
            int x = (int)MathF.Floor((context.cursorPosition.x + ratio) / (2 * ratio) * viewPort.width);
            int y = (int)MathF.Floor((context.cursorPosition.y + 1) / 2 * viewPort.height);
            return new Vector2i(
                Math.Clamp(x, 0, viewPort.width - 1),
                Math.Clamp(y, 0, viewPort.height - 1)
                );
        }

        int SamplePickable()
        {
            if (!hasPickingData)
                return 0;

            Framebuffer oldFb = context.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, pickingFbo);

            Vector2i pixel = CursorToPixel();
            float[] pickingIndex = new float[1];

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            ThrowErrors();

            GL.ReadPixels(pixel.x, pixel.y, 1, 1, PixelFormat.Red, PixelType.Float, pickingIndex);
            ThrowErrors();

            context.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, oldFb);

            int cast = (int)pickingIndex[0];
            if (cast != pickingIndex[0])
                throw new UnreachableException($"Picking index must always be an exact integer: {pickingIndex[0]}");

            return cast;
        }
    }
}
