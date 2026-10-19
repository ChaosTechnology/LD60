using System.Linq;
using ChaosFramework.Collections;
using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.ChaosShader;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Physics;
using OpenTK.Graphics.OpenGL;
using SysCol = System.Collections.Generic;

namespace LD60.World
{
    class World : Component<FloatingIslands>
    {
        record MapSegmentInfo(Map.Segment segment)
        {
            public Vector4f[] multitexIndices;
        }

        Physical physics;
        ShaderContainer.Entry shader;

        LinkedList<MapSegmentInfo> mapSegmemts = [];
        MultiLayerMaterial segmentMaterial;

        protected override void Create(CreateParameters c)
        {
            shader = scene.game.shaders.Load("shaders/picking_island.fx", this);

            physics = new(this, true);
            physics.isStatic = true;
            physics.state.contactFriction = 1.0f;
            physics.shapes.Clear();
            scene.physics.Add(physics);

            LinkedList<string>[] matNames = [[], [], [], []];
            SysCol.Dictionary<string, int>[] indexLookup = [[], [], [], []];
            string[] names = ["normal", "emissive", "diffuse", "specular"];
            foreach (Map.Segment segment in scene.map.EnumerateSegments())
            {
                if (segment.shapes != null)
                    physics.AddShape(segment.shapes);

                if (segment.mesh == null)
                    continue;

                mapSegmemts.Add(new MapSegmentInfo(segment));
                foreach (string matName in segment.usedMaterials)
                    for (int i = 0; i < names.Length; i++)
                    {
                        string assetKey = $"textures/multilayer/{matName}.{names[i]}.matlayer.png";
                        if (!indexLookup[i].ContainsKey(matName) && scene.game.multiMaterialLayers.ContainsKey(assetKey))
                        {
                            matNames[i].Add(assetKey);
                            indexLookup[i][matName] = matNames[i].length;
                        }
                    }
            }

            foreach (MapSegmentInfo segment in mapSegmemts)
                segment.multitexIndices = segment.segment.usedMaterials.Select(x => new Vector4f(
                    indexLookup[0].TryGetValue(x, out int idx) ? idx : 0,
                    indexLookup[1].TryGetValue(x, out idx) ? idx : 0,
                    indexLookup[2].TryGetValue(x, out idx) ? idx : 0,
                    indexLookup[3].TryGetValue(x, out idx) ? idx : 0
                    )).ToArray();

            segmentMaterial = new MultiLayerMaterial(
                scene.game.graphics.dispatcher,
                scene.game.multiMaterialLayers,
                matNames[0],
                matNames[1],
                matNames[2],
                matNames[3]
                );
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
            if (pass != "Picking")
                segmentMaterial.SetValues(shader);

            bool? picked = null;
            Shader.SemanticMapping semanticMapping = shader.BeginPass(pass);
            foreach(MapSegmentInfo info in mapSegmemts)
            {
                if (pass == "Picking")
                    shader.SetValue("pickingIndex", ((Pickable)info.segment).pickingIndex);
                else
                    shader.SetValue("materialIndices", info.multitexIndices);

                if (pass == "Material")
                {
                    bool p = scene.overlay.picked == info.segment;
                    if (picked == null || p != picked.Value)
                    {
                        picked = p;
                        shader.SetValue("picked", p ? 1.0f : 0.0f);
                    }
                }
                shader.content.CommitChanges();

                GL.BindVertexArray(info.segment.mesh.content.GetOrCreateVao(semanticMapping));
                Graphics.ThrowErrors();
                info.segment.mesh.content.Draw(shader, pass);
            }
            shader.EndPass();
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            segmentMaterial.Dispose();
        }
    }
}
