namespace LD60.World.Tactician.Tools
{
    public class Place
        : Tool
    {
        public override void Activate()
        {
            if (scene.overlay.picked is Map.Segment segment)
            {
                // TODO: select placables

                Placable brad = scene.AddComponent<Placables.Rock>();
                brad.physics.state.position = scene.overlay.worldPos;
            }
        }
    }
}
