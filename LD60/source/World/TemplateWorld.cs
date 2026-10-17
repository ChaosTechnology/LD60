using ChaosFramework.Graphics.Colors;
using ChaosFramework.Graphics.OpenGl.Lights;
using ChaosFramework.Input;
using ChaosFramework.Input.InputEvents;
using ChaosFramework.Math.Vectors;
using LD60.Input;

namespace LD60.World
{
    public class TemplateWorld : WorldScene
    {
        public readonly DirectionalLight sun;

        readonly Menu.PauseMenu pause;

        public TemplateWorld(Game game)
            : base (game)
        {
            sun = new DirectionalLight(
                deferredShader,
                new Vector3f(0.5f, -1, 1),
                new Rgba(0.9f, 0.9f, 0.9f, 1) * Confix.instance.brightness
                );
            sun.ambient = new Rgba(0.1f, 0.1f, 0.1f, 1) * Confix.instance.brightness;
            lights.Add(sun);

            AddComponent<World>();
            AddComponent<Skybox>();

            pause = new Menu.PauseMenu(this)
            {
                disposeOnSuicide = false
            };
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            game.input.AddKeyDownHandler(InputLayer.Game, Pause);
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
    }
}
