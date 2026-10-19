using ChaosFramework.Physics;
using ChaosUtil.Reflection;

namespace LD60.World.Tactician
{
    [AssemblyManager.ListSubTypes]
    public interface Placable
    {
        Physical physics { get; }
        void Init() { }
    }
}
