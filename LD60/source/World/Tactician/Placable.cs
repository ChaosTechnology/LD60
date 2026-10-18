using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Physics;
using ChaosUtil.Reflection;

namespace LD60.World.Tactician
{
    [AssemblyManager.ListSubTypes]
    public abstract class Placable
        : Component<FloatingIslands>
        , Instancable
        , Pickable
    {
        public readonly Physical physics;

        int Pickable.pickingIndex { get; set; }

        public Placable()
        {
            physics = new Physical(this, true);
        }

        protected override void Create(CreateParameters cparams)
        {
            scene.physics.Add(physics);
            scene.instancers[GetType()].instancedThings.Add(this);
            scene.overlay.Register(this);
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

        public virtual void GiveMeInstances(InstancingAttribute[] instancers)
            => instancers[0].informer.AddInstance(
                physics.state.GetTransform(),
                new Vector4f(
                    ((Pickable)this).pickingIndex,
                    scene.overlay.picked == this ? 1 : 0,
                    0,
                    0
                ));

        public bool NeedsInstancedDraw()
            => true; // TODO: cull out of screen

        protected override void DoDispose()
        {
            base.DoDispose();
            scene.physics.Remove(physics);
            scene.instancers[GetType()].instancedThings.Remove(this);
            scene.overlay.Unregister(this);
        }
    }
}
