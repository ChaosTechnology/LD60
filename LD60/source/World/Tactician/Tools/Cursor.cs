using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math;

namespace LD60.World.Tactician.Tools
{
    public class Cursor
        : Tool
    {
        ShaderContainer.Entry cursorShader;
        MeshContainer.Entry cursorMesh;

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            cursorShader = scene.game.graphics.shaders.normalMap;
            cursorMesh = scene.game.meshes.Load("models/cursor.gmdl", this);
        }

        public override void Activate()
        {
            // TODO: select unit, show details
        }

        protected override void SetDrawCalls2()
        {
            base.SetDrawCalls2();
            scene.drawLayers[(int)DrawLayers.WorldAfterPicking].Add(DrawCursorWorld);
            scene.drawLayers[(int)DrawLayers.Material].Add(DrawCursorMaterial);
        }

        void DrawCursorWorld()
            => DrawCursor("World");

        void DrawCursorMaterial()
            => DrawCursor("Material");

        void DrawCursor(string pass)
        {
            if (scene.overlay.picked is Map.Segment)
            {
                scene.view.SetValues(cursorShader, Matrix.Scaling(0.6f) * Matrix.Translation(scene.overlay.worldPos));
                scene.game.graphics.defaultMaterial.content.SetValues(cursorShader);
                cursorMesh.content.Draw(cursorShader, pass);
            }
        }
    }
}
