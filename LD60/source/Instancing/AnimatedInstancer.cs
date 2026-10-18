using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.ChaosShader;
using ChaosFramework.Graphics.OpenGl.Model;
using ChaosFramework.IO.Containers;
using ChaosFramework.Shapes;

namespace LD60.Instancing
{
    public class AnimatedInstancer
        : AnimatedInstancerBase
        , NormalMapped
    {
        protected InstancingHost host => Context<InstancingHost>();

        public AnimatedInstancer() { }
        public AnimatedInstancer(
            int maxInstances,
            string meshSource,
            string materialSource,
            string overrideEffect = null,
            params string[] customRegisters)
            : base(maxInstances,
                meshSource,
                materialSource,
                overrideEffect,
                customRegisters
            )
        { }

        public override void SetDrawCalls()
        {
            host.scene.drawLayers[(int)DrawLayers.World].Add(DrawWorld);
            host.scene.drawLayers[(int)DrawLayers.Material].Add(DrawMaterial);
        }

        void DrawWorld()
            => DrawInstances(host.view, "World");

        void DrawMaterial()
            => DrawInstances(host.view, "Material");

        protected override AssetContainer<Material>.Entry GetMaterial(string source)
            => host.game.materials.Load(source, host.scene);

        protected override AssetContainer<Mesh>.Entry GetMesh(string source)
            => host.game.meshes.Load(source, MeshLoadFlags.Animated, host.scene);

        protected override AssetContainer<Shader>.Entry GetShader(string source)
            => host.game.shaders.Load(source, host.scene);

        protected override void Initialize_2(Graphics graphics, object[] parameters)
        { }
    }
}
