using ChaosFramework.Components;

namespace LD60.World.Tactician.Placables
{
    [Tool("models/cube.gmdl", "models/rock/rock.mat")]
    [Instancing.PickingInstancer(3, "models/cube.gmdl", "models/rock/rock.mat")]
    public class Cube
        : Placable
    {
        public Cube()
            : base()
        {
            physics.state.mass = 100;
        }

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            physics.shapes.Add(scene.game.shapes.Load("models/cube.gmdl", this));
        }
    }
}
