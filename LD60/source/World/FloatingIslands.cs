using ChaosFramework.Components;
using ChaosFramework.Graphics.Colors;
using ChaosFramework.Graphics.OpenGl.Lights;
using ChaosFramework.Input;
using ChaosFramework.Input.InputEvents;
using ChaosFramework.Math.Vectors;
using LD60.Input;

namespace LD60.World
{
    public class FloatingIslands : WorldScene
    {
        public const float DEATH_PLANE = -100;

        public readonly DirectionalLight sun;

        readonly Menu.PauseMenu pause;

        public readonly Overlay overlay;
        public readonly Hostility hostility;
        public readonly Map map;

        bool mustInsertOverlay = true;

        public FloatingIslands(Game game)
            : base (game)
        {
            sun = new(
                deferredShader,
                new Vector3f(0.5f, -1, 1),
                new Rgba(0.9f, 0.9f, 0.9f, 1) * Confix.instance.brightness
                )
            {
                ambient = new Rgba(0.1f, 0.1f, 0.1f, 1) * Confix.instance.brightness
            };

            lights.Add(sun);

            overlay = new Overlay(this);
            overlay.SelectDefaultTool();

            map = new Map(this);
            hostility = AddComponent<Hostility>();

            AddComponent<World>();
            AddComponent<Skybox>();
            AddComponent<Player>();

            pause = new Menu.PauseMenu(this)
            {
                disposeOnSuicide = false
            };
        }

        public override void SetDrawCalls()
        {
            base.SetDrawCalls();
            drawLayers[(int)DrawLayers.BeginPicking].Add(overlay.BeginPicking);
        }

        public override void SetUpdateCalls()
        {
            if (mustInsertOverlay)
                InsertOverlay();

            base.SetUpdateCalls();
            game.input.AddKeyDownHandler(InputLayer.Game, Pause);
            game.input.AddMouseDownHandler(InputLayer.Tools, Tools);
        }

        bool Tools(InputPushEvent<Mouse.Button> e)
        {
            if (game.input.bindings.IsMatch(Inputs.ActivateTool, e.axis))
            {
                overlay.currentTool?.Activate();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Ensure that overlay is added to the scenes list AFTER this scene.
        ///     Can't be done in constructor, so do it during first update.
        /// </summary>
        void InsertOverlay()
        {
            mustInsertOverlay = false;

            DisposablesList<Scene> scenes = [];
            foreach (Scene s in game.scenes)
            {
                scenes.Add(s);
                if (s == this)
                    scenes.Add(overlay);
            }

            game.scenes.Clear();
            foreach(Scene s in scenes)
                game.scenes.Add(s);
        }

        bool Pause(InputPushEvent<Keyboard.Key> e)
        {
            if (game.input.bindings.IsMatch(Inputs.Pause, e.axis))
            {
                pause.ShowDialog(null, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            map.Dispose();
        }
    }
}
