using System.IO;
using System.Linq;
using ChaosFramework.Components;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Shapes.Rigging;
using SysCol = System.Collections.Generic;

namespace LD60.World
{
    public class Hostility : Component<FloatingIslands>
    {
        public Vector3f goal { get; private set; }

        protected override void Create(CreateParameters _)
        {
            using (Stream str = scene.game.archive.OpenRead("models/world2/goal.rig"))
                goal = Rig.FromStream(str).root.children.Single().GetPosition();

            SysCol.HashSet<(Vector3f, Matrix)> spawns = [];
            using (Stream str = scene.game.archive.OpenRead("models/world2/spawns.rig"))
                foreach (Rig.Bone bone in Rig.FromStream(str).root.children)
                    spawns.Add((bone.GetPosition(), bone.GetBoneTransform()));

            foreach ((Vector3f spawn, Matrix transform) in spawns)
            {
                Door door = AddComponent<Door>();
                door.physics.state.position = spawn;
                door.physics.state.baseTransform = transform * Matrix.Translation(-spawn);
            }
        }
    }
}
