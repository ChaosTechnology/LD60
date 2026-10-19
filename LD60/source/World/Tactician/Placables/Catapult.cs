using System.Linq;
using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Graphics.OpenGl.Model;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Shapes.Rigging;
using LD60.World.Enemies;
using static ChaosFramework.Math.Clamping;

namespace LD60.World.Tactician.Placables
{
    [Placable("models/catapult/catapult.agmdl", "models/catapult/catapult.mat", 100, 5)]
    [Instancing.AnimatedPickableInstancer(3, "models/catapult/catapult.agmdl", "models/catapult/catapult.mat")]
    public class Catapult
        : AnimatedPlacable
    {
        const float COOLDOWN = 3;
        static float MAX_TURNING_RADIANS_PER_SECOND = 9; // fast

        float shootProgress;
        float cooldownTimer = 1;
        float facingAngle;
        Goblin target = null;
        Vector3f launchPoint => physics.state.position + new Vector3f(0, 3, 0);

        public Catapult()
            : base()
        {
            physics.isStatic = true;
        }

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            physics.shapes.Add(scene.game.shapes.Load("models/catapult/catapult.agmdl", this));
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

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.Move].Add(UpdateState);
        }

        void UpdateState()
        {
            if (scene.phase.phase == Phase.Horde)
            {
                cooldownTimer -= ftime;
                if (cooldownTimer < 0.5f)
                {
                    if (!target?.alive != true || target?.health < 0)
                    {
                        Goblin[] candidates = scene.EnumerateChildren<Goblin>(true).ToArray();
                        target = candidates.MinBy(x => (x.physics.state.position - launchPoint).LengthSq());
                    }

                    if (target != null)
                        facingAngle = AngleHelper.TurnTowards(
                            facingAngle,
                            -(target.physics.state.position - physics.state.position), // randomly negate this lol
                            MAX_TURNING_RADIANS_PER_SECOND * ftime
                            );
                }
                if (cooldownTimer < 0.2f)
                    shootProgress = 0.9999f - Max(cooldownTimer / 0.2f, 0);

                if (cooldownTimer < 0)
                {
                    cooldownTimer = COOLDOWN;
                    if (target != null)
                    {
                        float estimatedTravelTime = (target.physics.state.position - launchPoint).Length() / 10.0f;
                        Vector3f estimatedTargetPosition = target.physics.state.position + target.physics.state.velocity * estimatedTravelTime;
                        Vector3f launchVelocity = ChaosFramework.Physics.ProjectileLaunchCalculator.GetLaunchVelocity(
                            estimatedTargetPosition - launchPoint,
                            10,
                            new(0, WorldScene.GRAVITY, 0),
                            ChaosFramework.Physics.ProjectileLaunchCalculator.AimMode.LowestSpeed
                            );
                        Rock rock = scene.AddComponent<Rock>();
                        rock.physics.Teleport(launchPoint);
                        rock.physics.state.velocity = launchVelocity;
                        rock.physics.state.contactFriction = 0.2f;
                        rock.physics.state.mass = 350;
                        rock.physics.shapes[0].bounce = 0.5f;
                        rock.physics.state.baseTransform = Matrix.Scaling(0.5f);
                    }
                }
            }

            physics.state.baseTransform = Matrix.RotationY(facingAngle);
        }

        protected override void UpdateAnimation()
        {
            base.UpdateAnimation();
            anim.content["Shoot"].SetAnimationData(rig, float.IsNaN(shootProgress) ? 0 : shootProgress);
        }
    }
}
