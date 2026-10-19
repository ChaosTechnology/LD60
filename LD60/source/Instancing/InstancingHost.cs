using ChaosFramework.Components;
using ChaosFramework.Graphics;

namespace LD60.Instancing
{
    public interface InstancingHost
    {
        Game game {get;}
        Scene scene {get;}
        Camera view {get;}
    }
}
