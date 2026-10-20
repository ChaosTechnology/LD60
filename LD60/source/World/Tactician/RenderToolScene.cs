using System;
using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.Lights;
using ChaosFramework.Math.Vectors;
using OpenTK.Graphics.OpenGL;

namespace LD60.World.Tactician
{
    public class ToolRenderer : DeferredRenderingScene
    {
        public ToolRenderer(FloatingIslands islands, int resolution)
            : base(islands.game, resolution, resolution)
        {
            lights.Add(new DirectionalLight(deferredShader, new Vector3f(1, 1, -1), islands.sun.color) { ambient = islands.sun.ambient });
        }

        public override void SetDrawCalls()
        {
            base.SetDrawCalls();
            drawLayers[(int)DrawLayers.BeginPicking].Clear();
            drawLayers[(int)DrawLayers.Picking].Clear();
            drawLayers[(int)DrawLayers.BeginWorldAfterPicking].Clear();
            drawLayers[(int)DrawLayers.Transparents].Clear();
            drawLayers[(int)DrawLayers.HUD].Clear();
            drawLayers[(int)DrawLayers.Sky].Add(Clear);
        }

        void Clear()
        {
            using(game.graphics.stateTracker.Start())
            {
                game.graphics.stateTracker.SetEnable(EnableCap.DepthTest, true);
                game.graphics.stateTracker.SetEnable(EnableCap.CullFace, false);
                game.graphics.stateTracker.SetEnable(EnableCap.Blend, false);
                game.graphics.stateTracker.SetRenderState<GlStateTracker.DepthMask, bool>(false);
                Sprite.DrawFullscreen(game.graphics, game.menuContext.texProvider.Load("button.png", this));
            }
        }

        public TextureContainer.Entry GetOrCreateImage(Type t)
        {
            string key = $"textures/menu/placables/{t.Name}.png";
            if (game.textures.TryLoad(key, out TextureContainer.Entry loaded, this))
                return loaded;

            const int RESOLUTION = 512;

            Texture.Parameters args = new(
                RESOLUTION,
                RESOLUTION,
                PixelType.UnsignedByte,
                TextureMinFilter.Linear,
                TextureMagFilter.Linear,
                PixelFormat.Rgba,
                PixelInternalFormat.Rgba8
                );

            Texture tex = new(game.graphics.dispatcher, args);
            using (Framebuffer buff = new(new ChaosFramework.Math.Bounds2i(0, 0, RESOLUTION, RESOLUTION), [tex]))
            using (Component c = AddComponent(t))
            {
                Placable placable = (Placable)c;
                placable.physics.UpdateShapes(true);

                Vector3f target = (placable.physics.boundingBoxLow + placable.physics.boundingBoxHigh) / 2;
                Vector3f dir = new Vector3f(0, -1, 1).GetNormalized();
                float dist = 1f;

                view.Update(target - dir * placable.physics.hullRadius * dist, dir, new Vector3f(0, 1, 0));

                Framebuffer old = game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, buff);
                Draw();
                game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, old);
            }

            game.textures.AddFactory(key, _ => tex);
            return game.textures.Load(key, this);
        }
    }
}
