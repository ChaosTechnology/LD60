using ChaosFramework.Math;
using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Physics;
using ChaosFramework.Core;
using ChaosFramework.Shapes.Convex;

namespace ChaosTemplates.Game3D.World
{
    [DefaultInstancer(1, "models/cube.gmdl", null)]
    class World : Component<TemplateWorld>, Instancable
    {
        Physical physics;

        protected override void Create(CreateParameters c)
        {
            physics = new(this, true);
            scene.physics.Add(physics);
            physics.isStatic = true;
            physics.shapes.Clear();
            scene.physics.Add(physics);
            using (Disposable x = new())
                physics.AddShape(new MeshShape([.. scene.game.meshes.Load("models/cube.gmdl", x).content.data.pos]));
            physics.state.position.y = -5;
            physics.state.baseTransform = Matrix.Scaling(50, 1, 50);

            scene.instancers.GetOrCreateManager(typeof(World)).instancedThings.Add(this);

            AddComponent<Player>();
        }

        void Instancable.GiveMeInstances(InstancingAttribute[] instancers)
            => instancers[0].informer.AddInstance(physics.state.GetTransform());

        bool Instancable.NeedsInstancedDraw()
            => true;

        protected override void DoDispose()
        {
            scene.instancers[typeof(World)].instancedThings.Remove(this);
            base.DoDispose();
        }
    }
}
