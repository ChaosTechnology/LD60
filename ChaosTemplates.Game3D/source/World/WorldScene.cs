using System;
using System.IO;
using ChaosFramework.Collections;
using ChaosFramework.Components;
using ChaosFramework.Graphics;
using ChaosFramework.Graphics.Imaging;
using ChaosFramework.Graphics.Imaging.Formats;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Graphics.OpenGl.Lights;
using ChaosFramework.Graphics.OpenGl.Lights.Intrinsic;
using ChaosFramework.Graphics.OpenGl.Text;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Platform;
using OpenTK.Graphics.OpenGL;
using static ChaosFramework.Math.Clamping;

namespace ChaosTemplates.Game3D.World
{
    public abstract class WorldScene : Scene<Game>
    {
        public const float GRAVITY = -9.81f;

        public Camera view { get; private set; }

        public DeferredShader deferredShader;
        public LightSet lights;
        public readonly DeferredShaderIntrinsicLights[] lightIntrinsics = [new DirectionalLightIntrinsics(1)];
        public readonly LightInstancerBase[] lightInstancers;

        public TransparencyRenderer transparency;
        public Transparents.ParticleFire fire;

        public readonly InstancingManagerContainer<WorldScene> instancers;
        public readonly ChaosFramework.Graphics.OpenGl.PostProcessors.AntiAliasing antiAliasing = new();

        public readonly LinkedList<Text> hudTexts, worldTexts, materialTexts;
        LinkedList<Tuple<DrawLayers, string, LinkedList<Text>>> textLists = [];

        public ChaosFramework.Physics.PhysicsWorld physics { get; private set; }

        Framebuffer postprocessingFBO;
        public Texture postprocessingTexture => postprocessingFBO.textures[0];

        protected bool takeScreenshot = false;

        public WorldScene(Game game, int width = -1, int height = -1) : base(game, typeof(UpdateLayers), typeof(DrawLayers))
        {
            textLists.Add(new Tuple<DrawLayers, string, LinkedList<Text>>(DrawLayers.World, "World", worldTexts = []));
            textLists.Add(new Tuple<DrawLayers, string, LinkedList<Text>>(DrawLayers.Material, "Material", materialTexts = []));
            textLists.Add(new Tuple<DrawLayers, string, LinkedList<Text>>(DrawLayers.HUD, "HUD", hudTexts = []));
            view = new Camera();
            view.Update(screenRatio: game.window.Ratio());
            game.graphics.windowsChanged += view.Update;

            lightInstancers =
            [
                new PointLightInstancer(game.graphics, 128)
            ];

            deferredShader = new DeferredShader(
                game.graphics,
                view,
                new Vector2i(game.window.width, game.window.height),
                lights = new LightSet(),
                lightIntrinsics,
                lightInstancers
                );

            transparency = new TransparencyRenderer(
                game.graphics,
                Max(1, (int)(deferredShader.width * Confix.instance.transparencyShaderScale)),
                Max(1, (int)(deferredShader.height * Confix.instance.transparencyShaderScale)),
                deferredShader.depthBuffer,
                (int)Confix.instance.numTransparencyLayers);
            transparency.solidDepthBias = 0.000025f;
            transparency.transparents.Add(fire = AddComponent<Transparents.ParticleFire>());

            antiAliasing = new ChaosFramework.Graphics.OpenGl.PostProcessors.AntiAliasing();
            antiAliasing.normalFactor = 6f;
            antiAliasing.positionFactor = .6f;

            instancers = new InstancingManagerContainer<WorldScene>(this, game.graphics);
            instancers.CreateInstancers(typeof(WorldScene).Assembly);

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

            CreatePhysics();
        }

        public void CreatePhysics()
        {
            physics?.Dispose();
            physics = AddComponent<ChaosFramework.Physics.PhysicsWorld>(CreateParameters.Create((int)UpdateLayers.Collision));
            physics.forces.Add(new ChaosFramework.Physics.Forces.LinearForce(0, GRAVITY, 0));
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();

            if (takeScreenshot)
                TakeScreenshot();

#if DEBUG
            for (int i = 0; i < updateLayers.Length; i++)
                updateLayers[i].Add(() => Graphics.ThrowErrors());
#endif
        }

        void TakeScreenshot()
        {
            string picFolder = System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.MyPictures,
                System.Environment.SpecialFolderOption.None
                );
            if (Directory.Exists(picFolder))
            {
                DirectoryInfo outFolder = new(Path.GetFullPath(picFolder) + "/ChaosTechnology/ChaosTemplates.Game3D/");
                FileInfo outFile;
                if (outFolder.Exists)
                    for (uint i = 0; (outFile = new FileInfo($"{outFolder.FullName}/{i}.png")).Exists; ++i);
                else
                {
                    outFolder.Create();
                    outFile = new FileInfo($"{outFolder.FullName}/0.png");
                }

                // allocate the file preemptively for name resolution sake
                Stream targetStr = outFile.Create();

                Rgba8Image img = game.graphics.TakeScreenshot(Draw);
                using (targetStr)
                    // TODO: do this in a background thread
                    Png.Save(img, targetStr);
            }
        }

        Framebuffer oeld;
        public override void SetDrawCalls()
        {
            foreach (var textList in textLists)
                textList.Item3.Clear();

            base.SetDrawCalls();

            drawLayers[(int)DrawLayers.StoreFrameBuffer].Add(() =>
            {
                oeld = game.graphics.stateTracker.GetFramebuffer(FramebufferTarget.Framebuffer);
            });
            drawLayers[(int)DrawLayers.ResetFramebuffer].Insert(0, () =>
            {
                game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, oeld);
            });

            drawLayers[(int)DrawLayers.BeginWorld].Add(BeginWorld);
            drawLayers[(int)DrawLayers.BeginMaterial].Add(deferredShader.BeginMaterial);

            instancers.SetDrawCalls(drawLayers[(int)DrawLayers.ResetInstancers], drawLayers[(int)DrawLayers.FillInstancers]);

            drawLayers[(int)DrawLayers.RenderDeferredShader].Add(() => deferredShader.Render());
            drawLayers[(int)DrawLayers.Transparents].Add(() => transparency.Render(deferredShader));
            drawLayers[(int)DrawLayers.AntiAliasing].Insert(0, () => ApplyAntiAliasing(oeld));

            foreach (var textList in textLists)
                drawLayers[(int)textList.Item1].Add(() =>
                {
                    view.SetValues(game.graphics.shaders.managedText, Matrix.IDENTITY);
                    game.textRenderer.DrawTexts(game.textBuffer, game.graphics.shaders.managedText, textList.Item2, textList.Item3);
                });

#if DEBUG
            for (int i = 0; i < drawLayers.Length; i++)
                drawLayers[i].Add(() => Graphics.ThrowErrors());
            Graphics.ThrowErrors();
#endif
        }

        void BeginWorld()
        {
            deferredShader.BeginWorld();
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void ApplyAntiAliasing(Framebuffer target)
        {
            game.graphics.stateTracker.BindFramebuffer(FramebufferTarget.Framebuffer, target);
            antiAliasing.Apply(deferredShader);
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            deferredShader.Dispose();
            game.graphics.windowsChanged -= view.Update;
            transparency.Dispose();
            instancers.Dispose();
        }
    }
}
