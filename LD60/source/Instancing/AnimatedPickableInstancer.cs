using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.ChaosShader;
using ChaosFramework.Graphics.OpenGl.Model;
using ChaosFramework.IO.Containers;
using ChaosFramework.Shapes;

namespace LD60.Instancing
{
    public class AnimatedPickableInstancer
        : AnimatedInstancerBase
    {
        protected InstancingHost host => Context<InstancingHost>();

        public AnimatedPickableInstancer() { }
        public AnimatedPickableInstancer(
            int maxInstances,
            string meshSource,
            string materialSource,
            string overrideEffect = "shaders/picking_skinned_instanced.fx",
            params string[] customRegisters)
            : base(
                maxInstances,
                meshSource,
                materialSource,
                overrideEffect,
                ["INSTANCE_ID", ..customRegisters]
            )
        { }

        public override void SetDrawCalls()
        {
            host.scene.drawLayers[(int)DrawLayers.World].Add(DrawWorld);
            host.scene.drawLayers[(int)DrawLayers.Material].Add(DrawMaterial);
            host.scene.drawLayers[(int)DrawLayers.Picking].Add(DrawPicking);
        }

        void DrawWorld()
            => DrawInstances(host.view, "World");

        void DrawMaterial()
            => DrawInstances(host.view, "Material");

        void DrawPicking()
            => DrawInstances(host.view, "Picking");

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
