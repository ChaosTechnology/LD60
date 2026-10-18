namespace LD60.World.Tactician.Placables
{
    [Tool("models/rock/rock.gmdl", "models/rock/rock.mat")]
    [Instancing.DefaultInstancer(3, "models/rock/rock.gmdl", "models/rock/rock.mat")]
    public class Rock
        : Placable
    {
        public Rock()
            : base()
        {
            physics.state.mass = 100;
        }
    }
}
