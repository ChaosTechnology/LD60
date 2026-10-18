using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Physics;

namespace LD60.World.Tactician
{
    public abstract class Placable
        : Component<FloatingIslands>
        , Instancable
    {
        public readonly Physical physics;

        public Placable()
        {
            physics = new Physical(this, true);
        }

        protected override void Create(CreateParameters cparams)
        {
            scene.physics.Add(physics);
            physics.shapes.Add(scene.game.shapes.Load("models/rock/rock.gmdl", this));
            scene.instancers[GetType()].instancedThings.Add(this);
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.Move].Add(Move);
            scene.updateLayers[(int)UpdateLayers.PostCollision].Add(ConsiderSuicide);
        }

        void Move()
            => physics.state.Move(ftime);

        void ConsiderSuicide()
        {
            if (physics.state.position.y < FloatingIslands.DEATH_PLANE)
                Dispose();
        }

        public void GiveMeInstances(InstancingAttribute[] instancers)
            => instancers[0].informer.AddInstance(physics.state.GetTransform());

        public bool NeedsInstancedDraw()
            => true; // TODO: cull out of screen

        protected override void DoDispose()
        {
            base.DoDispose();
            scene.physics.Remove(physics);
            scene.instancers[GetType()].instancedThings.Remove(this);
        }
    }
}
