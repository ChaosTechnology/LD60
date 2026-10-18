using ChaosFramework.Components;

namespace LD60.World.Tactician.Placables
{
    [Tool("models/rock/rock.gmdl", "models/rock/rock.mat")]
    [Instancing.PickingInstancer(3, "models/rock/rock.gmdl", "models/rock/rock.mat")]
    public class Rock
        : Placable
    {
        public Rock()
            : base()
        {
            physics.state.mass = 100;
        }

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            physics.shapes.Add(scene.game.shapes.Load("models/rock/rock.gmdl", this));
        }
    }
}
