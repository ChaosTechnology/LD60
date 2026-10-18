using ChaosFramework.Components;

namespace LD60.World.Tactician
{
    public abstract class Tool
        : Component<FloatingIslands>
    {
        protected override void Create(CreateParameters cparams)
        {
        }

        public abstract void Activate();
    }
}
