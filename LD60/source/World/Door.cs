using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Graphics.OpenGl.Model;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Shapes.Rigging;

namespace LD60.World
{
    using Instancing;

    [AnimatedPickableInstancer(3, "models/door/door.gmdl", "models/door/door.mat")]
    [DefaultInstancer(3, "models/door/void.gmdl", null, "models/door/void.fx", "TIME")]
    public class Door
        : StandardPickable
    {
        const float GOBLIN_SPAWN_INTERVAL = 3.0f;

        float goblinSpawnTimer = GOBLIN_SPAWN_INTERVAL;

        Rig rig;
        AnimationContainer.Entry animation;
        float open;

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            rig = Rig.FromStream(scene.game.archive.OpenRead("models/door/door.rig"));
            animation = scene.game.animations.Load("models/door/door.anim", this);
        }

        public override void GiveMeInstances(InstancingAttribute[] instancers)
        {
            Matrix transform = physics.state.GetTransform();

            AnimatedPickableInstancer animInstancer = (AnimatedPickableInstancer)instancers[0];
            animInstancer.AddInstance(
                rig.GetBoneTransforms(animInstancer.animesh),
                physics.state.GetTransform(),
                new Vector4f(((Pickable)this).pickingIndex, scene.overlay.picked == this ? 1 : 0, 0, 0)
                );

            if (open > 0)
                instancers[1].informer.AddInstance(transform, new Vector4f(-ftime.totalTime));
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.PrepareUpdate].Add(SpawnGoblins);
            scene.updateLayers[(int)UpdateLayers.CreateParticles].Add(UpdateAnimation);
        }

        void UpdateAnimation()
        {
            open = scene.phase.phase switch
            {
                Phase.Countdown => 1 - scene.phase.percentageLeft,
                Phase.Horde => 1,
                Phase.Victory => scene.phase.percentageLeft,
                _ => 0
            };

            rig.ClearAnimations();
            animation.content["Open"].SetAnimationData(rig, open);
        }

        void SpawnGoblins()
        {
            if ((goblinSpawnTimer -= ftime) <= 0)
            {
                goblinSpawnTimer += GOBLIN_SPAWN_INTERVAL;
                Enemies.Goblin john = scene.AddComponent<Enemies.Goblin>();
                john.physics.state.position = physics.state.position;
            }
        }
    }
}
