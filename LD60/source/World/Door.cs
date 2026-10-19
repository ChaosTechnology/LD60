using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Math;

namespace LD60.World
{
    using Instancing;

    [AnimatedPickableInstancer(3, "models/door/door.gmdl", "models/door/door.mat")]
    [DefaultInstancer(3, "models/door/void.gmdl", null)] // TODO: transparent wirling void
    public class Door
        : StandardPickable
    {
        const float GOBLIN_SPAWN_INTERVAL = 3.0f;

        float goblinSpawnTimer = GOBLIN_SPAWN_INTERVAL;

        public override void GiveMeInstances(InstancingAttribute[] instancers)
        {
            Matrix transform = physics.state.GetTransform();
            instancers[0].informer.AddInstance(transform, GetPickingRegister());
            instancers[1].informer.AddInstance(transform);
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
                john.physics.state.position = physics.state.position;
            }
        }
    }
}
