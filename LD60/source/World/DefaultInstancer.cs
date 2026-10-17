using ChaosFramework.Graphics;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Math;
using ChaosUtil.Platform.Paths;

namespace LD60.World
{
    class DefaultInstancer : InstancingAttribute
    {
        protected WorldScene scene { get; private set; }
        public ShaderContainer.Entry shader { get; protected set; }
        public MaterialContainer.Entry material { get; protected set; }
        public MeshContainer.Entry mesh { get; protected set; }

        public DefaultInstancer() { }
        public DefaultInstancer(
            int maxInstances,
            string meshSource,
            string materialSource,
            string overrideEffect = null,
            params string[] customRegisters
            ) : base(
                  maxInstances,
                  Normalization.NormalizeRelative(meshSource),
                  Normalization.NormalizeRelative(materialSource),
                  overrideEffect == null ? null : Normalization.NormalizeRelative(overrideEffect),
                  customRegisters
                  )
        { }

        protected DefaultInstancer(int maxInstances, object[] creationParams)
            : base(maxInstances, creationParams)
        { }

        public override void Initialize(Graphics graphics, int maxInstances, object[] parameters)
        {
            scene = Context<WorldScene>();

            if (parameters != null)
            {
                informer = new MatrixInstancer(scene.game.graphics, (string[])parameters[3], maxInstances);
                mesh = scene.game.meshes.Load((string)parameters[0], scene);
                material = parameters[1] == null
                    ? scene.game.graphics.defaultMaterial
                    : scene.game.materials.Load((string)parameters[1], scene);
                shader = parameters[2] == null
                    ? scene.game.graphics.shaders.instancedNormalMap
                    : scene.game.shaders.Load((string)parameters[2], scene);
            }
        }

        public virtual void DrawInstances(Camera view, string pass)
        {
            if (informer.numInstances == 0)
                return;

            view.SetValues(shader, Matrix.IDENTITY, Matrix.IDENTITY);
            material.content.SetValues(shader);
            mesh.content.DrawInstanced(shader, pass, informer);
        }

        public override void SetDrawCalls()
        {
            scene.drawLayers[(int)DrawLayers.World].Add(DrawWorld);
            scene.drawLayers[(int)DrawLayers.Material].Add(DrawMaterial);
        }

        void DrawWorld() => DrawInstances(scene.view, "World");
        void DrawMaterial() => DrawInstances(scene.view, "Material");
    }
}
