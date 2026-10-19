using ChaosFramework.Components;

namespace LD60.World.Tactician.Placables
{
    [Placable("models/menhir/menhir.gmdl", "models/menhir/menhir.mat", 50)]
    [Instancing.PickingInstancer(3, "models/menhir/menhir.gmdl", "models/menhir/menhir.mat")]
    public class Menhir
        : StandardPickable
        , Placable
    {
        public Menhir()
            : base()
        {
            physics.isStatic = true;
        }

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            physics.shapes.Add(scene.game.shapes.Load("models/menhir/menhir.gmdl", this));
        }
    }
}
