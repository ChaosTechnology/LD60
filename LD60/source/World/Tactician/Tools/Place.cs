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
using SysCol = System.Collections.Generic;

namespace LD60.World.Tactician.Tools
{
    using PlacementInfo = (NormalMapped nor, ShapeContainer.Entry shapes, Vector3f offset);

    public class Place
        : PlacableTool
    {
        readonly SysCol.Dictionary<Type, PlacementInfo> placementCache = [];

        Type placable = typeof(Rock);

        PlacementInfo GetPlacementInfo(Type placable)
        {
            if (!placementCache.TryGetValue(placable, out PlacementInfo placementInfo))
            {
                Vector3f offset = new(0, float.PositiveInfinity, 0);

                NormalMapped info = scene.instancers[placable].instancers.OfType<NormalMapped>().Single();
                ShapeContainer.Entry shapes = scene.game.shapes.Load(info.mesh.key.key, this);
                foreach(Shape shape in shapes.content)
                    offset.y = Math.Min(offset.y, shape.Support(new Vector3f(0, -1 ,0)).y);

                placementCache[placable] = placementInfo = (info, shapes, offset);
            }

            return placementInfo;
        }

        protected override void ModifyPlacableButton(Button btn, Type placable)
        {
            bool Enabled()
                => placable != this.placable && (scene.inventory.GetArsenal(placable) > 0);

            void Select(Control _)
                => this.placable = placable;

            btn.enabledFunction = Enabled;
            btn.mouseDownLeft += Select;
        }

        public override void Activate()
        {
            if (scene.overlay.picked is Map.Segment && placable != null)
                if (scene.inventory.GetArsenal(placable) > 0)
                {
                    scene.inventory.Use(placable);
                    Placable brad = (Placable)scene.AddComponent(placable);
                    PlacementInfo placementInfo = GetPlacementInfo(placable);
                    brad.physics.state.position = scene.overlay.worldPos - placementInfo.offset;
                    brad.Init();
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
                PlacementInfo placementInfo = GetPlacementInfo(placable);
                shader.SetValue("color", scene.inventory.GetArsenal(placable) > 0
                    ? new Rgba(0, 0.5f, 0, 1)
                    : new Rgba(0.5f, 0, 0, 1)
                    );
                scene.view.SetValues(shader, Matrix.Translation(scene.overlay.worldPos - placementInfo.offset));
                placementInfo.nor.material.content.SetValues(shader);
                placementInfo.nor.mesh.content.Draw(shader, pass);
            }
        }
    }
}
