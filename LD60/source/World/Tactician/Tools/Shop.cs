using System;
using System.Linq;
using ChaosFramework.Menu.OpenGl;
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
        }

        public override void Activate()
        { }

        public override bool Allowed()
            => scene.phase.phase == Phase.Strategize;
    }
}
