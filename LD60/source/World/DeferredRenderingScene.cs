using ChaosFramework.Components;
using ChaosFramework.Graphics;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Graphics.OpenGl.Lights;
using ChaosFramework.Graphics.OpenGl.Lights.Intrinsic;
using ChaosFramework.Math.Vectors;
using LD60.Instancing;
using OpenTK.Graphics.OpenGL;

namespace LD60.World
{
    public class DeferredRenderingScene
        : Scene<Game>
        , InstancingHost
    {
        public Camera view { get; private set; }

        public DeferredShader deferredShader;
        public LightSet lights;
        public readonly DeferredShaderIntrinsicLights[] lightIntrinsics = [new DirectionalLightIntrinsics(1)];
        public readonly LightInstancerBase[] lightInstancers;

        public readonly InstancingManagerContainer<InstancingHost> instancers;
        public readonly ChaosFramework.Graphics.OpenGl.PostProcessors.AntiAliasing antiAliasing = new();

        readonly Framebuffer postprocessingFBO;
        public Texture postprocessingTexture => postprocessingFBO.textures[0];

        Scene InstancingHost.scene => this;

        public DeferredRenderingScene(Game game, int width, int height)
            : base(game, typeof(UpdateLayers), typeof(DrawLayers))
        {
            view = new Camera();
            view.Update(screenRatio: (float)width / height);
            game.graphics.windowsChanged += view.Update;

            lightInstancers =
            [
                new PointLightInstancer(game.graphics, 128)
            ];

            deferredShader = new DeferredShader(
                game.graphics,
                view,
                new Vector2i(width, height),
                lights = new LightSet(),
                lightIntrinsics,
                lightInstancers
                );

            antiAliasing = new ChaosFramework.Graphics.OpenGl.PostProcessors.AntiAliasing
            {
                normalFactor = 6f,
                positionFactor = .6f
            };

            instancers = new InstancingManagerContainer<InstancingHost>(this, game.graphics);
            instancers.CreateInstancers(typeof(DeferredRenderingScene).Assembly);

            postprocessingFBO = new Framebuffer(deferredShader.width, deferredShader.height, [
                new Texture(
                    game.graphics.dispatcher,
                    new Texture.Parameters(
                       deferredShader.width,
                       deferredShader.height,
                       pixelType: PixelType.Float,
                       pixelFormat: PixelFormat.Rgba,
                       internalFormat: PixelInternalFormat.Rgba32f
                       )
                    )
            ]);
        }

#if DEBUG
        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();

            for (int i = 0; i < updateLayers.Length; i++)
                updateLayers[i].Add(ThrowErrors);
        }
#endif

        Framebuffer oldFb;
        public override void SetDrawCalls()
        {
            base.SetDrawCalls();

            drawLayers[(int)DrawLayers.StoreFrameBuffer].Add(StoreFramebuffer);
            drawLayers[(int)DrawLayers.ResetFramebuffer].Insert(0, ResetFramebuffer);

            drawLayers[(int)DrawLayers.BeginWorld].Add(BeginWorld);
            drawLayers[(int)DrawLayers.BeginWorldAfterPicking].Add(BeginWorldAfterPicking);
            drawLayers[(int)DrawLayers.BeginMaterial].Add(deferredShader.BeginMaterial);

            instancers.SetDrawCalls(drawLayers[(int)DrawLayers.ResetInstancers], drawLayers[(int)DrawLayers.FillInstancers]);

            drawLayers[(int)DrawLayers.RenderDeferredShader].Add(RenderDeferred);
            drawLayers[(int)DrawLayers.AntiAliasing].Insert(0, ApplyAntiAliasing);

#if DEBUG
            for (int i = 0; i < drawLayers.Length; i++)
                drawLayers[i].Add(ThrowErrors);

            ThrowErrors();
#endif
        }

#if DEBUG
        void ThrowErrors()
            => Graphics.ThrowErrors();
#endif

        void StoreFramebuffer()
            => oldFb = game.graphics.stateTracker.GetFramebuffer(FramebufferTarget.Framebuffer);

        void ResetFramebuffer()
            => game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, oldFb);

        protected virtual void RenderDeferred()
            => deferredShader.Render();

        void BeginWorld()
        {
            deferredShader.BeginWorld();
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        void BeginWorldAfterPicking()
            => game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, deferredShader.worldFramebuffer);

        void ApplyAntiAliasing()
        {
            game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, oldFb);
            antiAliasing.Apply(deferredShader);
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            deferredShader.Dispose();
            game.graphics.windowsChanged -= view.Update;
            instancers.Dispose();
        }
    }
}
