using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Physics;

namespace LD60.World
{
    public abstract class StandardPickable
        : Component<DeferredRenderingScene>
        , Pickable
        , Instancable
    {
        readonly Physical _physics;

        protected FloatingIslands islands => scene as FloatingIslands;

        int Pickable.pickingIndex { get; set; }
        public Physical physics => _physics;

        public StandardPickable()
        {
            _physics = new Physical(this, true)
            {
                isStatic = true
            };
        }

        protected override void Create(CreateParameters cparams)
        {
            islands?.physics.Add(physics);
            islands?.overlay.Register(this);
            scene.instancers[GetType()].instancedThings.Add(this);
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.Move].Add(Move);
            scene.updateLayers[(int)UpdateLayers.PostCollision].Add(ConsiderSuicide);
        }

        protected virtual void Move()
            => physics.state.Move(ftime);

        protected virtual void ConsiderSuicide()
        {
            if (physics.state.position.y < FloatingIslands.DEATH_PLANE)
                Dispose();
        }

        protected Vector4f GetPickingRegister()
            => new(((Pickable)this).pickingIndex, islands?.overlay.picked == this ? 1 : 0, 0, 0);

        public virtual void GiveMeInstances(InstancingAttribute[] instancers)
            => instancers[0].informer.AddInstance(physics.state.GetTransform(), GetPickingRegister());

        bool Instancable.NeedsInstancedDraw()
            => true; // TODO: cull out of screen

        protected override void DoDispose()
        {
            base.DoDispose();
            islands?.physics.Remove(physics);
            islands?.overlay.Unregister(this);
            scene.instancers[GetType()].instancedThings.Remove(this);
        }
    }
}
