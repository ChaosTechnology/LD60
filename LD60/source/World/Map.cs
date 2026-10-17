using System.Linq;
using ChaosFramework.Collections.Immutable;
using ChaosFramework.Core;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Math;
using ChaosFramework.Shapes.Convex;
using ChaosFramework.Shapes.Rigging;
using SysCol = System.Collections.Generic;
using ChaosFramework.Physics;

namespace LD60.World
{
    public class Map : Disposable
    {
        public abstract class Segment
        {
            public readonly MeshContainer.Entry mesh;
            public readonly ShapeContainer.Entry shapes;
            public readonly ShapeContainer.Entry home;

            public Segment(Map map, string source)
            {
                mesh = map.game.meshes.Load($"{source}.gmdl", map);
                shapes = map.game.shapes.Load($"{source}.gmdl", map);

                home = map.game.shapes.Load($"{source}.area.gmdl", map);
                foreach(Shape s in home.content)
                    s.Update(Matrix.IDENTITY, true);
            }
        }

        public class Island(Map map, string source, params Path[] outgoing)
            : Segment(map, source)
        {
            public readonly ImmutableArray<Path> outgoing = outgoing;
        }

        public class Path
            : Segment
        {
            public readonly Island destination;
            public readonly Rig rig;
            public readonly Vector3f end, start;

            public Path(Map map, string source, Island destination)
                : base(map, source)
            {
                this.destination = destination;
                rig = Rig.FromStream(map.game.archive.OpenRead($"{source}.rig"));

                Rig.Bone endBone = rig.root.children.Single(), nextBone;
                start = endBone.GetPosition();
                while ((nextBone = endBone.children.SingleOrDefault()) != null)
                    endBone = nextBone;
                end = endBone.GetPosition() + endBone.GetDirection() * endBone.length;
            }
        }

        const string WORLD = "models/world/";

        public readonly ImmutableArray<Island> islands;
        public readonly ImmutableArray<Path> paths;

        readonly Game game;

        public Map(Game game)
        {
            this.game = game;

            // TODO: load from some config file

            Island goal = new(this, $"{WORLD}island.x");
            Path pathA = new(this, $"{WORLD}path.a", goal);
            Path pathB = new(this, $"{WORLD}path.b", goal);
            Island start = new(this, $"{WORLD}island.1", pathA, pathB);

            islands = new[] { start, goal };
            paths = new[] { pathA, pathB };
        }

        public SysCol.IEnumerable<Segment> EnumerateSegments()
            => [..islands, ..paths];
    }
}
