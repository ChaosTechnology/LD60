using System;
using System.Linq;
using ChaosFramework.Graphics.Colors;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Physics;
using ChaosFramework.Shapes.Convex;
using LD60.Instancing;
using LD60.Tactician;
using LD60.World.Tactician.Placables;
using SysCol = System.Collections.Generic;

namespace LD60.World.Tactician.Tools
{
    using PlacementInfo = (NormalMapped nor, ShapeContainer.Entry shapes, Vector3f offset);

    public class Place
        : PlacableTool
    {
        readonly SysCol.Dictionary<Type, PlacementInfo> placementCache = [];

        // TODO: capture TypeWithAttribute<PlacableAttribute> here
        public Type placable = typeof(Rock);

        public float cooldownTime, cooldownTimer = 0;

        PlacementInfo GetPlacementInfo(Type placable)
        {
            if (!placementCache.TryGetValue(placable, out PlacementInfo placementInfo))
            {
                Vector3f offset = new(0, float.PositiveInfinity, 0);

                NormalMapped info = scene.instancers[placable].instancers.OfType<NormalMapped>().Single();
                ShapeContainer.Entry shapes = scene.game.shapes.Load(info.mesh.key.key, this);
                foreach (Shape shape in shapes.content)
                    offset.y = Math.Min(offset.y, shape.Support(new Vector3f(0, -1, 0)).y);

                placementCache[placable] = placementInfo = (info, shapes, offset);
            }

            return placementInfo;
        }

        protected override void ModifyPlacableButton(ToolButton btn, Type placable)
        {
            bool Enabled()
                => placable != this.placable && (scene.inventory.GetArsenal(placable) > 0);

            void Select(Control _)
                => this.placable = placable;

            (bool, float) GetToolInfo()
                => (this.placable == placable, cooldownTimer / cooldownTime);

            btn.getToolInfo = GetToolInfo;
            btn.enabledFunction = Enabled;
            btn.mouseDownLeft += Select;
        }

        public override void Select()
        {
            base.Select();
            if (placable == null || scene.inventory.GetArsenal(placable) <= 0)
            {
                placable = null;
                foreach (ChaosUtil.Reflection.TypeWithAttribute<PlacableAttribute> p in placables)
                    if (scene.inventory.GetArsenal(p.type) > 0)
                    {
                        placable = p.type;
                        break;
                    }
            }
        }

        public override void Activate()
        {
            if (placable != null
                && cooldownTimer <= 0
                && scene.overlay.picked is Map.Segment
                && scene.inventory.GetArsenal(placable) > 0
                )
            {
                scene.inventory.Use(placable);
                Placable brad = (Placable)scene.AddComponent(placable);
                PlacementInfo placementInfo = GetPlacementInfo(placable);
                brad.physics.state.position = scene.overlay.worldPos - placementInfo.offset;
                brad.Init();

                if (scene.phase.phase != Phase.Strategize)
                    cooldownTime = cooldownTimer = placables.Single(t => t.type == placable).attribute.cooldown;
            }
        }

        protected override void SetUpdateCalls2()
        {
            base.SetUpdateCalls2();
            scene.updateLayers[(int)UpdateLayers.UpdateParticles].Add(DecrementCooldown);
        }

        void DecrementCooldown()
            => cooldownTimer -= ftime;

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
            if (scene.overlay.picked is Map.Segment && placable != null)
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
