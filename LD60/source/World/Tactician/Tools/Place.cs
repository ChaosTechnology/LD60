using System;
using System.Linq;
using ChaosFramework.Graphics.Colors;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosFramework.Physics;
using ChaosFramework.Shapes.Convex;
using LD60.Instancing;
using LD60.World.Tactician.Placables;
using Microsoft.VisualBasic;

namespace LD60.World.Tactician.Tools
{
    public class Place
        : Tool
    {
        Type placable = typeof(Cube);

        public override void Select()
        {
            base.Select();
            CreatePlacableButtons(ModifyPlacableButton);
        }

        void ModifyPlacableButton(Button btn, Type placable)
        {
            bool Enabled()
                => placable != this.placable;

            void Select(Control _)
                => this.placable = placable;

            btn.enabledFunction = Enabled;
            btn.mouseDownLeft += Select;
        }

        public override void Activate()
        {
            if (scene.overlay.picked is Map.Segment && placable != null)
            {
                Placable brad = (Placable)scene.AddComponent(placable);
                brad.physics.state.position = scene.overlay.worldPos;
            }
        }

        protected override void SetDrawCalls2()
        {
            base.SetDrawCalls2();
            scene.drawLayers[(int)DrawLayers.WorldAfterPicking].Add(DrawWorld);
            scene.drawLayers[(int)DrawLayers.Material].Add(DrawMaterial);
        }

        void DrawWorld()
            => Draw("World");

        void DrawMaterial()
            => Draw("Material");

        void Draw(string pass)
        {
            if (scene.overlay.picked is Map.Segment)
            {
                ShaderContainer.Entry shader = scene.game.graphics.shaders.coloredNormalMap;
                NormalMapped info = scene.instancers[placable].instancers.OfType<NormalMapped>().Single();

                ShapeContainer.Entry shapes = scene.game.shapes.Load(info.mesh.key.key, this);
                Vector3f offset = new(0, float.PositiveInfinity, 0);
                foreach(Shape shape in shapes.content)
                    offset.y = Math.Min(offset.y, shape.Support(new Vector3f(0, -1 ,0)).y);

                shader.SetValue("color", new Rgba(0, 0.5f, 0, 1));
                scene.view.SetValues(shader, Matrix.Translation(scene.overlay.worldPos - offset));
                info.material.content.SetValues(shader);
                info.mesh.content.Draw(shader, pass);
            }
        }
    }
}
