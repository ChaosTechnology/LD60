using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Shapes.Rigging;

namespace LD60.World.Tactician
{
    public abstract class AnimatedPlacable
        : Placable
    {
        protected Rig rig;
        protected AnimationContainer.Entry anim;

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.ClearAnimations].Add(ClearAnimation);
            scene.updateLayers[(int)UpdateLayers.UpdateAnimations].Add(UpdateAnimation);
        }

        void ClearAnimation()
        {
            rig.ClearAnimations();
        }

        protected virtual void UpdateAnimation()
        { }
    }
}
