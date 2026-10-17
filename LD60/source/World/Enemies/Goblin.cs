using System.Diagnostics;
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
using LD60.Menu;
using static ChaosFramework.Math.Constants;
using static ChaosFramework.Math.Exponentials;

namespace LD60.World.Enemies
{
    [Instancing.AnimatedInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.1.mat")]
    [Instancing.AnimatedInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.2.mat")]
    [Instancing.AnimatedInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.3.mat")]
    public class Goblin : Component<FloatingIslands>, Instancable
    {
        static readonly float GOBLIN_MOVE_FAST = 3;

        public readonly Physical physics;

        readonly int kind = Random.instance.RndInt(3);

        Vector3f interesting = Vector3f.NAN;
        Rig rig;
        AnimationContainer.Entry animation;
        float walkProgress;
        float facingAngle = 0;

        public Goblin()
        {
            physics = new Physical(this);
            physics.shapes.Add(new CapsuleShape(new Vector3f(0, 0.5f, 0), new Vector3f(0, 1, 0), 0.5f));
        }

        protected override void Create(CreateParameters _)
        {
            scene.physics.Add(physics);
            rig = Rig.FromStream(scene.game.archive.OpenRead("models/goblin/goblin.rig"));
            animation = scene.game.animations.Load("models/goblin/goblin.anim", this);
            scene.instancers[typeof(Goblin)].instancedThings.Add(this);
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.Move].Add(Move);
            scene.updateLayers[(int)UpdateLayers.PostCollision].Add(ConsiderSuicide);
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
                                float d = (outgoing.end - physics.state.position).LengthSq();
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
                    throw new UnreachableException();
            }

            if (interesting.LengthSq() < float.PositiveInfinity)
            {
                Vector2f fast = Vector2f.Normalize(interesting.xz - physics.state.position.xz) * GOBLIN_MOVE_FAST;
                physics.state.velocity.xz += (fast - physics.state.velocity.xz) * EaseIn(ftime * 60);
            }

            if ((physics.state.position - scene.hostility.goal).LengthSq() < 1)
                new Death(scene).ShowDialog(null, null);

            facingAngle = (float)System.Math.Atan2(physics.state.velocity.x, physics.state.velocity.z);
            physics.state.baseTransform = Matrix.RotationY(facingAngle + PI);
            physics.state.Move(ftime);
            rig.ClearAnimations();
            walkProgress = (walkProgress + ftime * physics.state.velocity.xz.Length()) % 1;
            animation.content["Walk"].SetAnimationData(rig, walkProgress);
        }

        void ConsiderSuicide()
        {
            if (physics.state.position.y < -100)
                Dispose();
        }

        void Instancable.GiveMeInstances(InstancingAttribute[] instancers)
        {
            Instancing.AnimatedInstancer animInstancer = (Instancing.AnimatedInstancer)instancers[kind];
            animInstancer.AddInstance(rig.GetBoneTransforms(animInstancer.animesh), physics.state.GetTransform());
        }

        bool Instancable.NeedsInstancedDraw()
            => true; // TODO: out of screen

        protected override void DoDispose()
        {
            base.DoDispose();
            scene.physics.Remove(physics);
            scene.instancers[typeof(Goblin)].instancedThings.Remove(this);
        }
    }
}
