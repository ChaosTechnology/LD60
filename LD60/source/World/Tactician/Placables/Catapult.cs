using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Graphics.OpenGl.Model;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Shapes.Rigging;

namespace LD60.World.Tactician.Placables
{
    [Tool("models/catapult/catapult.gmdl", "models/catapult/catapult.mat")]
    [Instancing.AnimatedPickableInstancer(3, "models/catapult/catapult.gmdl", "models/catapult/catapult.mat")]
    public class Catapult
        : AnimatedPlacable
    {
        float shootProgress;

        public Catapult()
            : base()
        {
            physics.isStatic = true;
        }

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            physics.shapes.Add(scene.game.shapes.Load("models/catapult/catapult.gmdl", this));
            rig = Rig.FromStream(scene.game.archive.OpenRead("models/catapult/catapult.rig"));
            anim = scene.game.animations.Load("models/catapult/catapult.anim", this);
        }

        public override void GiveMeInstances(InstancingAttribute[] instancers)
        {
            Instancing.AnimatedPickableInstancer animInstancer = (Instancing.AnimatedPickableInstancer)instancers[0];
            animInstancer.AddInstance(
                rig.GetBoneTransforms(animInstancer.animesh),
                physics.state.GetTransform(),
                new Vector4f(((Pickable)this).pickingIndex, scene.overlay.picked == this ? 1 : 0, 0, 0)
                );
        }

        protected override void UpdateAnimation()
        {
            base.UpdateAnimation();
            shootProgress = (shootProgress + ftime) % 1; // TODO: shoot, not loop
            anim.content["Shoot"].SetAnimationData(rig, float.IsNaN(shootProgress) ? 0 : shootProgress);
        }
    }
}
