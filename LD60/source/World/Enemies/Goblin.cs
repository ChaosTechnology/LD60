using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Physics;
using ChaosFramework.Shapes.Convex;
using ChaosUtil.Primitives;

namespace LD60.World.Enemies
{
    [Instancing.AnimatedInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.1.mat")]
    [Instancing.AnimatedInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.2.mat")]
    [Instancing.AnimatedInstancer(666, "models/goblin/goblin.agmdl", "models/goblin/goblin.3.mat")]
    public class Goblin : Component<WorldScene>, Instancable
    {
        public readonly Physical physics;

        readonly int kind = Random.instance.RndInt(3);

        public Goblin()
        {
            physics = new Physical(this);
            physics.shapes.Add(new CapsuleShape(new Vector3f(0, 0.5f, 0), new Vector3f(0, 1, 0), 0.5f));
        }

        protected override void Create(CreateParameters _)
        {
            scene.physics.Add(physics);
            scene.instancers[typeof(Goblin)].instancedThings.Add(this);
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.Move].Add(Move);
            scene.updateLayers[(int)UpdateLayers.PostCollision].Add(ConsiderSuicide);
        }

        void Move()
        {
            physics.state.Move(ftime);
        }

        void ConsiderSuicide()
        {
            if (physics.state.position.y < -100)
                Dispose();
        }

        void Instancable.GiveMeInstances(InstancingAttribute[] instancers)
            => instancers[kind].informer.AddInstance(physics.state.GetTransform());

        bool Instancable.NeedsInstancedDraw()
            => true; // TODO: out of screen

        protected override void DoDispose()
        {
            base.DoDispose();
            scene.physics.Remove(physics);
            scene.instancers[typeof(Goblin)].instancedThings.Remove(this);
        }
    }
}
