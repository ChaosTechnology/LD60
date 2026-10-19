using ChaosFramework.Components;

namespace LD60.World.Tactician.Placables
{
    [Placable("models/rock/rock.gmdl", "models/rock/rock.mat", 10)]
    [Instancing.PickingInstancer(3, "models/rock/rock.gmdl", "models/rock/rock.mat")]
    public class Rock
        : StandardPickable
        , Placable
    {
        public Rock()
            : base()
        {
            physics.state.mass = 5000;
            physics.state.contactFriction = 0.5f;
            physics.isStatic = false;
        }

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            physics.shapes.Add(scene.game.shapes.Load("models/rock/rock.gmdl", this));
            physics.shapes[0].bounce = 0;
        }

        void Placable.Init()
            => physics.state.position.y += 10;
    }
}
