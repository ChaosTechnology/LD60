using System.Linq;
using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Physics;
using ChaosFramework.Shapes.Convex;
using ChaosUtil.Primitives;
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

        public Goblin()
        {
            physics = new Physical(this);
            physics.shapes.Add(new CapsuleShape(new Vector3f(0, 0.5f, 0), new Vector3f(0, 1, 0), 0.5f));
        }

        protected override void Create(CreateParameters _)
        {
            scene.physics.Add(physics);
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
            Map.Segment residence = null;
            foreach (Map.Segment segment in scene.map.EnumerateSegments())
                foreach(Shape q in segment.home.content)
                    foreach(Shape s in physics.shapes)
                        if (ChaosFramework.Shapes.Intersection.ConvexHull.CheckIntersection(q, s) != null)
                            if (residence == null || (residence as Map.Path)?.destination == segment)
                                residence = segment;
                            else if ((segment as Map.Path)?.destination == residence)
                                continue;

            if (residence is Map.Path path)
            {
                // TODO: actually follow path
                interesting = path.end;
            }
            else if (residence is Map.Island island)
            {
                if (island.outgoing.Any())
                {
                    interesting = Vector3f.POSITIVE_INFINITY;
                    float distOfInteresting = float.PositiveInfinity;
                    foreach(Map.Path outgoing in island.outgoing)
                    {
                        float d = (outgoing.end -physics.state.position).LengthSq();
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
            }

            if (interesting.Length() < float.PositiveInfinity)
            {
                Vector2f fast = Vector2f.Normalize(interesting.xz - physics.state.position.xz) * GOBLIN_MOVE_FAST;
                physics.state.velocity.xz += (fast - physics.state.velocity.xz) * EaseIn(ftime * 60);
            }

            if ((physics.state.position - scene.hostility.goal).LengthSq() < 1)
                Dispose();

            physics.state.Move(ftime);
        }

        void ConsiderSuicide()
        {
            if (physics.state.position.y < -100)
                Dispose();
        }

        void Instancable.GiveMeInstances(InstancingAttribute[] instancers)
            => instancers[kind].informer.AddInstance(physics.state.GetTransform());

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
