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
        ShaderContainer.Entry shader;

        protected override void Create(CreateParameters c)
        {
            shader = scene.game.shaders.Load("shaders/picking.fx", this);

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
            scene.drawLayers[(int)DrawLayers.Picking].Add(DrawPicking);
        }

        void DrawWorld()
            => Draw("World");

        void DrawMaterial()
            => Draw("Material");

        void DrawPicking()
            => Draw("Picking");

        void Draw(string pass)
        {
            scene.view.SetValues(shader, Matrix.IDENTITY, Matrix.IDENTITY);
            scene.game.graphics.defaultMaterial.content.SetValues(shader);

            bool? picked = null;
            Shader.SemanticMapping semanticMapping = shader.BeginPass(pass);
            foreach(Map.Segment segment in scene.map.EnumerateSegments())
            {
                if (pass == "Picking")
                {
                    shader.SetValue("pickingIndex", ((Pickable)segment).pickingIndex);
                    shader.content.CommitChanges();
                }

                if (pass == "Material")
                {
                    bool p = scene.overlay.picked == segment;
                    if (picked == null || p != picked.Value)
                    {
                        picked = p;
                        shader.SetValue("picked", p ? 1.0f : 0.0f);
                        shader.content.CommitChanges();
                    }
                }

                GL.BindVertexArray(segment.mesh.content.GetOrCreateVao(semanticMapping));
                Graphics.ThrowErrors();
                segment.mesh.content.Draw(shader, pass);
            }
            shader.EndPass();
        }
    }
}
