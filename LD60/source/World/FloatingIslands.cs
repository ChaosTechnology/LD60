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
        public readonly DirectionalLight sun;

        readonly Menu.PauseMenu pause;

        public readonly Hostility hostility;
        public readonly Map map;

        bool needOverlay = true;

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

            map = new Map(game);
            hostility = AddComponent<Hostility>();

            AddComponent<World>();
            AddComponent<Skybox>();
            AddComponent<Player>();

            pause = new Menu.PauseMenu(this)
            {
                disposeOnSuicide = false
            };
        }

        public override void SetUpdateCalls()
        {
            if (needOverlay)
                CreateOverlay();

            base.SetUpdateCalls();
            game.input.AddKeyDownHandler(InputLayer.Game, Pause);
        }

        void CreateOverlay()
        {
            needOverlay = false;

            DisposablesList<Scene> scenes = [];
            foreach (Scene s in game.scenes)
            {
                scenes.Add(s);
                if (s == this)
                    scenes.Add(new Overlay(game.menuContext));
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
