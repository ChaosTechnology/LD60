using System.IO;
using System.Linq;
using ChaosFramework.Collections.Immutable;
using ChaosFramework.Core;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Physics;
using ChaosFramework.Shapes.Convex;
using ChaosFramework.Shapes.Rigging;
using SysCol = System.Collections.Generic;

namespace LD60.World
{
    public class Map : Disposable
    {
        public abstract class Segment : Pickable
        {
            static readonly char[] LINE_BREAKS = ['\r', '\n'];

            public readonly MeshContainer.Entry mesh;
            public readonly ShapeContainer.Entry shapes;
            public readonly ShapeContainer.Entry home;
            public readonly string[] usedMaterials;

            int Pickable.pickingIndex { get; set; }

            public Segment(Map map, string source)
            {
                map.scene.overlay.Register(this);

                using (StreamReader materialsReader = new(map.scene.game.archive.OpenRead($"{source}/mesh.gmdl.materials")))
                    usedMaterials = materialsReader.ReadToEnd().Split(LINE_BREAKS, System.StringSplitOptions.RemoveEmptyEntries);

                mesh = map.scene.game.meshes.Load($"{source}/mesh.gmdl", map);
                shapes = map.scene.game.shapes.Load($"{source}/shapes.obj", map);

                if (!map.scene.game.shapes.TryLoad($"{source}/area.obj", out home, map))
                    home = map.scene.game.shapes.Load($"{source}/shapes.obj", map);

                foreach(Shape s in home.content)
                    s.Update(Matrix.Translation(0, 1, 0), true);
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
                rig = Rig.FromStream(map.scene.game.archive.OpenRead($"{source}/path.rig"));

                Rig.Bone endBone = rig.root.children.Single(), nextBone;
                start = endBone.GetPosition();
                while ((nextBone = endBone.children.SingleOrDefault()) != null)
                    endBone = nextBone;
                end = endBone.GetPosition() + endBone.GetDirection() * endBone.length;
            }

            public SysCol.IEnumerable<(Vector3f pos, Vector3f dir)> KeyPoints()
            {
                Rig.Bone e = rig.root.children.Single(), n = e;
                for (; n != null; n = n.children.SingleOrDefault())
                    yield return ((e = n).GetPosition(), e.GetDirection());

                Vector3f d = e.GetDirection();
                yield return (e.GetPosition() + d * e.length, d);
            }
        }

        const string WORLD = "models/world2/";

        public readonly ImmutableArray<Island> islands;
        public readonly ImmutableArray<Path> paths;

        readonly FloatingIslands scene;

        public Map(FloatingIslands scene)
        {
            this.scene = scene;

            // TODO: load from some config file

            Island treasure = new(this, $"{WORLD}island.treasure");
            Path middleToTreasure = new(this, $"{WORLD}bridge.middle_to_treasure", treasure);
            Path sideToTreasure = new(this, $"{WORLD}bridge.side_to_treasure", treasure);

            Island middle = new(this, $"{WORLD}island.middle", middleToTreasure);
            Path spawn3ToMiddle = new(this, $"{WORLD}bridge.spawn3_to_middle", middle);
            Path spawn2ToMiddle = new(this, $"{WORLD}bridge.spawn2_to_middle", middle);

            Island spawn1 = new(this, $"{WORLD}island.spawn1", spawn3ToMiddle); // TODO: the island should be called spawn3

            Island sidevector = new(this, $"{WORLD}island.sidevector", sideToTreasure);
            Path tinyToSide = new(this, $"{WORLD}bridge.middle_to_side", middle); // TODO: the path should be called tiny_to_side
            Island tiny = new(this, $"{WORLD}island.tiny", tinyToSide);

            Path spawn1ToTiny = new(this, $"{WORLD}bridge.spawn3_to_tiny", middle); // TODO: the path should be called spawn1_to_tiny
            Path spawn2ToTiny = new(this, $"{WORLD}bridge.spawn2_to_tiny", middle);

            Island spawn3 = new(this, $"{WORLD}island.spawn3", spawn1ToTiny);
            Island spawn2 = new(this, $"{WORLD}island.spawn2", spawn2ToMiddle, spawn2ToTiny);

            islands = new[] { middle, treasure, sidevector, spawn1, spawn2, spawn3, tiny };
            paths = new [] { middleToTreasure, sideToTreasure, spawn3ToMiddle, spawn2ToMiddle, tinyToSide, spawn1ToTiny, spawn2ToTiny };
        }

        public SysCol.IEnumerable<Segment> EnumerateSegments()
            => [..islands, ..paths];
    }
}
