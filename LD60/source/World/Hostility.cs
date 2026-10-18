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
        const float GOBLIN_SPAWN_INTERVAL = 3.0f;

        public Vector3f goal {get; private set;}
        public ImmutableArray<Vector3f> spawns {get; private set;}

        float goblinSpawnTimer = GOBLIN_SPAWN_INTERVAL;

        protected override void Create(CreateParameters _)
        {
            using (Stream str = scene.game.archive.OpenRead("models/world2/goal.rig"))
                goal = Rig.FromStream(str).root.children.Single().GetPosition();

            SysCol.HashSet<Vector3f> spawns = [];
            using (Stream str = scene.game.archive.OpenRead("models/world2/spawns.rig"))
                foreach(Rig.Bone bone in Rig.FromStream(str).root.children)
                    spawns.Add(bone.GetPosition());
            this.spawns = spawns.ToArray();
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.PrepareUpdate].Add(SpawnGoblins);
        }

        void SpawnGoblins()
        {
            if ((goblinSpawnTimer -= ftime) <= 0)
            {
                goblinSpawnTimer += GOBLIN_SPAWN_INTERVAL;
                Enemies.Goblin john = scene.AddComponent<Enemies.Goblin>();
                john.physics.state.position = spawns.RandomEntry();
            }
        }
    }
}
