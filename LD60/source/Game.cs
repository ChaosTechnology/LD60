using System;
using System.Threading.Tasks;
using ChaosFramework.Components;
using ChaosFramework.Core;
using ChaosFramework.Graphics.AssetContainers;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.Model;
using ChaosFramework.Graphics.OpenGl.Text;
using ChaosFramework.Input;
using ChaosFramework.IO;
using ChaosFramework.IO.Streams;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Physics;
using ChaosFramework.Platform;
using ChaosFramework.Shapes;
using ChaosFramework.Sound;
using ChaosFramework.Sound.OpenAL;
using ChaosUtil.Reflection;
using LD60.World.Tactician;
using OpenTK.Graphics.OpenGL;

namespace LD60
{
    public class Game : BaseGame
    {
        public const string SPRITE = "$Sprite";

        public ChaosArchive archive { get; private set; }

        public Graphics graphics { get; private set; }
        public Audio audio { get; private set; }
        public Music music { get; private set; }

        public TextureContainer textures { get; private set; }
        public Rgba8ImageContainer multiMaterialLayers { get; private set; }
        public MaterialContainer materials { get; private set; }
        public FontContainer fonts { get; private set; }
        public MeshContainer meshes { get; private set; }
        public AnimationContainer animations { get; private set; }
        public ShaderContainer shaders { get; set; }
        public ShaderCodeContainer shaderCode { get; set; }
        public ShapeContainer shapes { get; private set; }
        public SoundDataContainer samples { get; private set; }

        public MenuContext menuContext { get; private set; }
        public TextRenderer textRenderer { get; private set; }
        public TextRenderer.TextBuffer textBuffer { get; private set; }
        public SoundPool soundPool { get; private set; }

        public FontContainer.Entry defaultFont { get; private set; }
        public LanguagePack language { get; private set; }

        public readonly PresentationContext window;
        public readonly PlatformContext platformContext;
        public readonly Input.Input input;

        public Game(PlatformContext platformContext, PresentationContext window, Func<InputContext, InputDeviceHost> createInputContext)
            : base(platformContext.messageQueue)
        {
            this.window = window;
            this.platformContext = platformContext;
            this.input = new Input.Input(createInputContext);
            gameLoop = new ChaosFramework.Components.GameLoop.CappedVariableTimeLoop(platformContext.messageQueue, Confix.instance.frameRate);
            platformContext.Terminate += Terminate;
        }

        void RunTasks(params Action[] actions)
            => RunTasks(Array.ConvertAll(actions, Task.Run));

        void RunTasks(params Task[] t)
        {
            Task initializer = Task.WhenAll(t);
            while (!initializer.IsCompleted)
            {
                platformContext.messageQueue();
                Dispatcher.dispatcher.ExecuteDispatchers(1000);
            }

            if (!initializer.IsCompletedSuccessfully)
                throw initializer.Exception;
        }

        public override void LoadGame()
        {
            base.LoadGame();

            void RenderLoadingScreen(Graphics graphics)
            {
                using (Texture t = Texture.FromStream(graphics.dispatcher, Properties.Resources.ResourceManager.GetStream("loading")))
                    Sprite.DrawFullscreen(graphics, t);

                window.Present();
            }

            Dispatcher dispatcher = Dispatcher.dispatcher;
            RunTasks(
                Task.Run(() =>
                {
                    archive = new ChaosArchive(new System.IO.FileInfo("assets.cha"), false);
                    audio = new Audio();
                    music = new Music(audio, archive.OpenRead("music/music.ogg"), true);
                    music.Play(Confix.instance.musicVolume * Confix.instance.masterVolume);
                }),
                Task.Run(() => graphics = new Graphics(dispatcher, platformContext.glContext, 3, 3, RenderLoadingScreen)),
                Task.Run(input.Init)
                );

            const bool monitoring = false;
            async Task LoadShaders()
            {
                shaderCode = new ShaderCodeContainer(new StreamSourceCollection(archive, StreamSources.shaderCode));
                await shaderCode.LoadAllAsync(@"^*\.c?fx$", this);
                await (shaders = new ShaderContainer(archive, graphics, shaderCode, monitoring, graphics.coreProfile))
                    .LoadAllAsync(@"^*\.fx$", this);
            }
            RunTasks(
                (textures = new TextureContainer(archive, graphics.dispatcher, monitoring)).LoadAllAsync(@"^.*(?!\.matlayer)\.png$", this)
                    .ContinueWith(_ => (materials = new MaterialContainer(archive, graphics, textures, monitoring)).LoadAllAsync(@"^.*\.mat$", this)),
                (multiMaterialLayers = new Rgba8ImageContainer(archive, monitoring)).LoadAllAsync(@"^.*matlayer\.png$", this),
                (meshes = new MeshContainer(archive, graphics.dispatcher, monitoring))
                    .LoadAllAsync(MeshLoadFlags.Default, @"^.*\.gmdl$", this)
                    .ContinueWith(_ => meshes.LoadAll(MeshLoadFlags.Animated, @"^.*\.agmdl$", this))
                    .ContinueWith(_ => meshes.AddFactory(SPRITE, _ => new Mesh(graphics.dispatcher, MeshData.sprite)))
                    .ContinueWith(_ => meshes.Load(SPRITE, this)),
                (animations = new AnimationContainer(archive, monitoring)).LoadAllAsync(@"^.*\.anim$", this),
                (samples = new SoundDataContainer(archive, monitoring)).LoadAllAsync(@"^.*\.(wav|ogg)$", this),
                (shapes = new ShapeContainer(archive)).LoadAllAsync(@"^.*\.obj$", this)
                    .ContinueWith(_ =>
                    {
                        foreach(TypeWithAttribute<PlacableAttribute> placable in PlacableTool.placables)
                            shapes.Load(placable.attribute.mesh, this);
                    }),
                (fonts = new FontContainer(archive, graphics, monitoring)).LoadAllAsync(@"^fonts/.*\.chf2$", this),
                Task.Run(() => language = new LanguagePack(archive, "english", "english")),
                LoadShaders()
                );

            soundPool = new SoundPool(audio, samples);

            defaultFont = fonts.Load("fonts/font.chf2", this);
            textRenderer = TextRenderer.Create(graphics, 1000, 10000, 1000, false, defaultFont);
            textBuffer = textRenderer.CreateRenderContext();
            TextureProvider texProvider = new(graphics, textures);
            menuContext = new MenuContext(this, graphics, window, input.context, InputLayer.Menu, language, defaultFont, texProvider);

            scenes.Add(new Menu.MainMenu(menuContext));
        }

        protected override void Update()
        {
            float ftime = time.time.frameTime;
            float vol = Confix.instance.musicVolume * Confix.instance.masterVolume;
            if (vol != music.volume)
                music.volume = vol;

            Dispatcher.dispatcher.ExecuteDispatchers(10);
            soundPool.Update();

            input.UpdateDeviceRefresh(time.time);
            menuContext.Update(time.time);
            input.context.UpdateInputConsumption();

            base.Update();
        }

        protected override void Draw()
        {
            GL.ClearColor(0.0f, ChaosUtil.Primitives.Random.instance.Rnd(), 0.0f, 1.0f);
            Graphics.ThrowErrors();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            Graphics.ThrowErrors();
            base.Draw();
            window.Present();
        }

        protected override void DoDispose()
        {
            platformContext.Terminate -= Terminate;
            base.DoDispose();
            RunTasks(
                input.Dispose,
                shaders.Dispose,
                shaderCode.Dispose,
                animations.Dispose,
                meshes.Dispose,
                textures.Dispose,
                samples.Dispose,
                materials.Dispose,
                fonts.Dispose,
                soundPool.Dispose,
                music.Dispose,
                textRenderer.Dispose
                );
            RunTasks(
                audio.Dispose,
                graphics.Dispose,
                archive.Dispose
                );
        }
    }
}
