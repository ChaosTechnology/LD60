using System;
using System.Linq;
using ChaosFramework.Graphics.Colors;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosUtil.Reflection;
using LD60.Tactician;

namespace LD60.World.Tactician.Tools
{
    public class Shop
        : PlacableTool
    {
        protected override void ModifyPlacableButton(ToolButton btn, Type placable)
        {
            TypeWithAttribute<PlacableAttribute> info = placables.Single(t => t.type == placable);

            bool Enabled()
                => info.attribute.gold <= scene.inventory.gold;

            void Buy(Control _)
            {
                if (scene.inventory.gold < info.attribute.gold)
                    throw new InvalidOperationException("Can't afford that.");

                scene.inventory.gold -= info.attribute.gold;
                scene.inventory.AddStock(placable, 1);
                scene.EnumerateChildren<Place>(true).Single().placable = info.type;
            }

            btn.enabledFunction = Enabled;
            btn.mouseDownLeft += Buy;

            scene.overlay.panel.Layout(); // TODO: Don't
            float h = btn.width / 3;
            float l = btn.position.x - btn.width / 2;
            float m = btn.position.x - btn.width / 6;
            float y = btn.position.y + btn.height / 2 + h / 2;

            Image img1 = scene.overlay.CreateControl<Image>(new Vector2f((m + l) / 2, y), h / 2);
            img1.textureSource = "models/coin/menu.png";
            img1.text = string.Empty;

            float r = btn.position.x + btn.width / 2;
            float f = 1.5f; // TODO: fix text fitting instead
            Image img2 = scene.overlay.CreateControl<Image>(new Vector2f((m + r) / 2, y), new Vector2f(h, h / 2) * f);
            img2.textureSource = string.Empty;
            img2.text = info.attribute.gold.ToString();
            img2.textColor = new Rgba(1, 0.8f, 0.4f, 1);
            img2.textAlign = ChaosFramework.Graphics.Align.Center;
            img2.textFit = Control.TextFit.Scale;
            img2.textMargin = 0;

            void KillLables()
            {
                img1.Dispose();
                img2.Dispose();
                btn.RemoveOnDispose(KillLables);
            }

            btn.AddOnDispose(KillLables);
        }

        public override void Activate()
        { }

        public override bool Allowed()
            => scene.phase.phase == Phase.Strategize;
    }
}
