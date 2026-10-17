using ChaosFramework.Collections;
using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.ChaosShader;
using ChaosFramework.Math;
using ChaosFramework.Physics;
using OpenTK.Graphics.OpenGL;

namespace LD60.World
{
    class World : Component<FloatingIslands>
    {
        Physical physics;

        protected override void Create(CreateParameters c)
        {
            physics = new(this, true);
            scene.physics.Add(physics);
            physics.isStatic = true;
            physics.shapes.Clear();
            scene.physics.Add(physics);

            LinkedList<MeshContainer.Entry> meshes = [];
            foreach (Map.Segment segment in scene.map.EnumerateSegments())
                physics.AddShape(segment.shapes);
        }

        public override void SetDrawCalls()
        {
            base.SetDrawCalls();
            scene.drawLayers[(int)DrawLayers.World].Add(DrawWorld);
            scene.drawLayers[(int)DrawLayers.Material].Add(DrawMaterial);
        }

        void DrawWorld()
            => Draw("World");

        void DrawMaterial()
            => Draw("Material");

        void Draw(string pass)
        {
            ShaderContainer.Entry shader = scene.game.graphics.shaders.normalMap;
            scene.view.SetValues(shader, Matrix.IDENTITY, Matrix.IDENTITY);
            scene.game.graphics.defaultMaterial.content.SetValues(shader);

            Shader.SemanticMapping semanticMapping = shader.BeginPass(pass);
            foreach(Map.Segment segment in scene.map.EnumerateSegments())
            {
                GL.BindVertexArray(segment.mesh.content.GetOrCreateVao(semanticMapping));
                Graphics.ThrowErrors();
                segment.mesh.content.Draw(shader, pass);
            }
            shader.EndPass();
        }
    }
}
