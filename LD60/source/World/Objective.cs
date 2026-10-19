using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Math;

namespace LD60.World
{
    using Instancing;

    [PickingInstancer(3, "models/beer/beer.gmdl", "models/beer/beer.mat")]
    public class Objective
        : StandardPickable
    {
        public override void GiveMeInstances(InstancingAttribute[] instancers)
        {
            Matrix transform = physics.state.GetTransform();
            instancers[0].informer.AddInstance(transform, GetPickingRegister());
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.PrepareUpdate].Add(CheckLoseCondition);
        }

        void CheckLoseCondition()
        {
        }
    }
}
