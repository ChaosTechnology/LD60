using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Math;
using ChaosFramework.Physics;

namespace LD60.World
{
    using Enemies;
    using Instancing;
    using Menu;

    [PickingInstancer(3, "models/beer/beer.gmdl", "models/beer/beer.mat")]
    public class Objective
        : StandardPickable
    {
        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            physics.shapes.Clear();
            physics.shapes.Add(scene.game.shapes.Load("models/beer/beer.gmdl", this));
        }

        public override void GiveMeInstances(InstancingAttribute[] instancers)
        {
            Matrix transform = physics.state.GetTransform();
            instancers[0].informer.AddInstance(transform, GetPickingRegister());
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.PostCollision].Add(CheckLoseCondition);
        }

        void CheckLoseCondition()
        {
            foreach (CollisionData collision in physics.currentCollisions)
                if ((collision.p1.creator == this ? collision.p2.creator : collision.p1.creator) is Goblin)
                    Lose();
        }

        void Lose()
            => new Death(islands).ShowDialog(null, null);
    }
}
