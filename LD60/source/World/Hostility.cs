using System.IO;
using ChaosFramework.Components;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Shapes.Rigging;

namespace LD60.World
{
    public class Hostility : Component<FloatingIslands>
    {
        public Vector3f goal { get; private set; }
        protected override void Create(CreateParameters _)
        {
            using (Stream str = scene.game.archive.OpenRead("models/world2/spawns.rig"))
                foreach (Rig.Bone bone in Rig.FromStream(str).root.children)
                {
                    Door door = AddComponent<Door>();
                    door.physics.state.position = bone.GetPosition();
                    door.physics.state.baseTransform = bone.GetBoneTransform() * Matrix.Translation(-door.physics.state.position);
                }

            using (Stream str = scene.game.archive.OpenRead("models/world2/goal.rig"))
                foreach (Rig.Bone bone in Rig.FromStream(str).root.children)
                {
                    Objective beer = AddComponent<Objective>();
                    beer.physics.state.position = bone.GetPosition();
                    beer.physics.state.baseTransform =
                        Matrix.Scaling(bone.length)
                        * bone.GetBoneTransform()
                        * Matrix.Translation(-beer.physics.state.position);
                    goal = beer.physics.state.position;
                }
        }
    }
}
