using System.IO;
using System.Linq;
using ChaosFramework.Collections.Immutable;
using ChaosFramework.Components;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Shapes.Rigging;
using SysCol = System.Collections.Generic;

namespace LD60.World
{
    public class Hostility : Component<FloatingIslands>
    {
        public Vector3f goal { get; private set; }
        public ImmutableArray<Vector3f> spawns { get; private set; }

        protected override void Create(CreateParameters _)
        {
            using (Stream str = scene.game.archive.OpenRead("models/world2/goal.rig"))
                goal = Rig.FromStream(str).root.children.Single().GetPosition();

            SysCol.HashSet<Vector3f> spawns = [];
            using (Stream str = scene.game.archive.OpenRead("models/world2/spawns.rig"))
                foreach (Rig.Bone bone in Rig.FromStream(str).root.children)
                    spawns.Add(bone.GetPosition());
            this.spawns = spawns.ToArray();

            foreach (Vector3f spawn in spawns)
            {
                Door door = AddComponent<Door>();
                door.physics.state.position = spawn;
            }
        }
    }
}
