using ChaosFramework.Collections;
using ChaosFramework.Collections.Immutable;
using ChaosFramework.Components;
using ChaosFramework.Core;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.ChaosShader;
using ChaosFramework.IO.Streams;
using ChaosFramework.Math;
using ChaosFramework.Physics;
using ChaosFramework.Shapes.Convex;
using OpenTK.Graphics.OpenGL;

namespace LD60.World
{
    class World : Component<FloatingIslands>
    {
        Physical physics;
        ImmutableArray<MeshContainer.Entry> meshes;

        protected override void Create(CreateParameters c)
        {
            physics = new(this, true);
            scene.physics.Add(physics);
            physics.isStatic = true;
            physics.shapes.Clear();
            scene.physics.Add(physics);

            LinkedList<MeshContainer.Entry> meshes = [];
            foreach (string island in scene.game.archive.EnumerateKeys("models/world/*.*.gmdl"))
            {
                MeshContainer.Entry mesh = scene.game.meshes.Load(island, this);
                meshes.Add(mesh);
                physics.AddShape(new MeshShape([.. mesh.content.data.pos]));
            }
            this.meshes = meshes.ToArray();
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
            foreach(MeshContainer.Entry mesh in meshes)
            {
                GL.BindVertexArray(mesh.content.GetOrCreateVao(semanticMapping));
                Graphics.ThrowErrors();
                mesh.content.Draw(shader, pass);
            }
            shader.EndPass();
        }
    }
}
