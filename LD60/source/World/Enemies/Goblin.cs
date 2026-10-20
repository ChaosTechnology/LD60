using System.Linq;
using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Graphics.OpenGl.Model;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Physics;
using ChaosFramework.Shapes.Convex;
using ChaosFramework.Shapes.Rigging;
using ChaosUtil.Primitives;
using LD60.World.Tactician.Placables;
using static ChaosFramework.Math.Constants;
using static ChaosFramework.Math.Exponentials;

namespace LD60.World.Enemies
{
    [Instancing.AnimatedPickableInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.1.mat")]
    [Instancing.AnimatedPickableInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.2.mat")]
    [Instancing.AnimatedPickableInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.3.mat")]
    public class Goblin : Component<FloatingIslands>, Instancable, Pickable
    {
        const float MAX_TURNING_RADIANS_PER_SECOND = 2.0f;

        static float RandomAmbientTime()
            => Random.instance.Rnd(2.5f, 8.7f) * 4;

        public float health { get; private set; } = 100.0f;

        float scale = 1.0f;
        float determination = 1.0f;

        float TARGET_SPEED => 2.5f * determination;
        float SPEED_KEEP_AHEAD => 1.1f;
        float WALKING_STRENGTH => 4 * physics.state.mass;


        public readonly Physical physics;

        float ambientNoiseTimer = RandomAmbientTime();
        bool screaming = false;

        readonly int kind = Random.instance.RndInt(3);

        Vector3f interesting = Vector3f.NAN;
        Rig rig;
        AnimationContainer.Entry animation;
        float walkProgress;
        float facingAngle = 0;

        int Pickable.pickingIndex {get; set;}

        public Goblin()
        {
            physics = new Physical(this);
            physics.shapes.Add(new CapsuleShape(new Vector3f(0, 0.5f, 0), new Vector3f(0, 1, 0), 0.5f));
            physics.state.contactFriction = 0.1f;
        }

        protected override void Create(CreateParameters _)
        {
            scene.physics.Add(physics);
            rig = Rig.FromStream(scene.game.archive.OpenRead("models/goblin/goblin.rig"));
            animation = scene.game.animations.Load("models/goblin/goblin.anim", this);
            scene.instancers[typeof(Goblin)].instancedThings.Add(this);
            scene.overlay.Register(this);

            float scaleBase = EaseIn(scene.phase.wave * 0.3f);
            float speedBase = 0.5f + scene.phase.wave * 0.2f;
            determination = Random.instance.Rnd(speedBase, speedBase * 1.5f);
            scale = Random.instance.Rnd(1.5f, 1 + scaleBase * scaleBase * 3.0f);
            physics.state.mass = 40.0f * scale;
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.Move].Add(Move);
            scene.updateLayers[(int)UpdateLayers.Move].Add(Groan);
            scene.updateLayers[(int)UpdateLayers.PostCollision].Add(ConsiderSuicide);
        }

        void Groan()
        {
            if ((ambientNoiseTimer -= ftime) < 0)
            {
                ambientNoiseTimer = RandomAmbientTime();
                scene.enemySounds.Play(Sounds.Kind.GoblinAmbient);
            }
        }

        void Move()
        {
            Vector3f pos = physics.state.position;
            Map.Segment residence = null;
            foreach (Map.Segment segment in scene.map.EnumerateSegments())
                foreach(Shape segmentShape in segment.home.content)
                    foreach(Shape homeShape in physics.shapes)
                        if (ChaosFramework.Shapes.Intersection.ConvexHull.CheckIntersection(segmentShape, homeShape) != null)
                            if (residence == null
                                || (residence as Map.Path)?.destination == segment
                                || residence is Map.Island island && island.outgoing.Any(x => x == segment)
                                )
                                residence = segment;
                            else if (residence is Map.Path residencePath && segment is Map.Path segmentPath)
                            {
                                float closestPath = float.PositiveInfinity;
                                foreach(Map.Path y in new[]{residencePath, segmentPath})
                                    foreach ((Vector3f p, Vector3f _) in y.KeyPoints())
                                    {
                                        float d = (p - pos).LengthSq();
                                        if (d < closestPath)
                                        {
                                            closestPath = d;
                                            residence = y;
                                        }
                                    }
                            }


            switch (residence)
            {
                case Map.Path path:
                    bool found = false;
                    foreach((Vector3f pos, Vector3f dir) keypoint in path.KeyPoints())
                    {
                        float d = Vector3f.Dot(keypoint.dir, keypoint.pos);
                        ChaosFramework.Math.Plane p = new(keypoint.dir, d);
                        if (p.Dot(pos) < 0)
                        {
                            found = true;
                            interesting = keypoint.pos;
                            break;
                        }
                    }

                    if (!found)
                        interesting = path.KeyPoints().First().pos;

                    break;

                case Map.Island island:
                    {
                        if (island.outgoing.Any())
                        {
                            interesting = Vector3f.POSITIVE_INFINITY;
                            float distOfInteresting = float.PositiveInfinity;
                            foreach (Map.Path outgoing in island.outgoing)
                            {
                                float d = (outgoing.start - physics.state.position).LengthSq();
                                if (d < distOfInteresting)
                                {
                                    distOfInteresting = d;
                                    interesting = outgoing.start;
                                }
                            }
                        }
                        else
                        {
                            interesting = scene.hostility.goal;
                        }

                        break;
                    }

                default:
                    interesting = scene.hostility.goal;
                    break;
            }

            if (interesting.LengthSq() < float.PositiveInfinity)
            {
                Vector2f fast = Vector2f.Normalize(interesting.xz - physics.state.position.xz) * TARGET_SPEED;
                Vector2f delta = fast * SPEED_KEEP_AHEAD - physics.state.velocity.xz;
                physics.state.ApplyForce(new Vector3f(delta.x, 0, delta.y) * WALKING_STRENGTH, physics.state.position, ftime);
            }

            facingAngle = AngleHelper.TurnTowards(facingAngle, physics.state.velocity, MAX_TURNING_RADIANS_PER_SECOND * ftime);

            physics.state.baseTransform = Matrix.Scaling(scale) * Matrix.RotationY(facingAngle + PI);
            physics.state.Move(ftime);
            rig.ClearAnimations();
            walkProgress = (walkProgress + ftime * physics.state.velocity.xz.Length()) % 1;
            animation.content["Walk"].SetAnimationData(rig, walkProgress);
        }

        void ConsiderSuicide()
        {
            foreach (CollisionData collision in physics.currentCollisions)
            {
                Physical other = collision.p1.creator == this ? collision.p2 : collision.p1;
                if (other.creator is Rock oha)
                    foreach (CollisionData.ShapeCollisionData data in collision.data)
                        {
                            float damage = (float)System.Math.Log2((data.v1 - data.v2).Length() * other.state.mass);
                            if (damage > 1.0f)
                            {
                                health -= damage;
                                scene.enemySounds.Play(Sounds.Kind.GoblinDamage);
                            }
                        }
            }
            if (physics.state.position.y < FloatingIslands.SCREAM_PLANE && !screaming)
            {
                screaming = true;
                scene.enemySounds.Play(Sounds.Kind.GoblinFalling);
                DropMoney(10, true);
            }

            if (physics.state.position.y < FloatingIslands.DEATH_PLANE)
                Dispose();
            else if (health < 0)
            {
                Die();
                DropMoney(25, false);
            }
        }

        void Die()
        {
            // TODO: fall over and turn into corpse
            Dispose();
            scene.fire.Explode(physics.state.position, 12, new(1.0f, 1.7f, 0.2f));
        }

        void DropMoney(int coinValue, bool autocollect)
        {
            Coin c = scene.AddComponent<Coin>();
            c.value = coinValue;
            Vector3f off = c.physics.shapes[0].Support(new Vector3f(0, -1, 0));
            c.physics.state.position = physics.state.position - new Vector3f(0, off.y, 0);
            if (autocollect)
                c.Collect();
        }

        void Instancable.GiveMeInstances(InstancingAttribute[] instancers)
        {
            Instancing.AnimatedPickableInstancer animInstancer = (Instancing.AnimatedPickableInstancer)instancers[kind];
            animInstancer.AddInstance(
                rig.GetBoneTransforms(animInstancer.animesh),
                physics.state.GetTransform(),
                new Vector4f(((Pickable)this).pickingIndex, scene.overlay.picked == this ? 1 : 0, 0, 0)
                );
        }

        bool Instancable.NeedsInstancedDraw()
            => true; // TODO: out of screen

        protected override void DoDispose()
        {
            base.DoDispose();
            scene.physics.Remove(physics);
            scene.instancers[typeof(Goblin)].instancedThings.Remove(this);
            scene.overlay.Unregister(this);
        }
    }
}
