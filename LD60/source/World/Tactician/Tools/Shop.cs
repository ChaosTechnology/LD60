using System;
using System.Linq;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosUtil.Reflection;

namespace LD60.World.Tactician.Tools
{
    public class Shop
        : PlacableTool
    {
        protected override void ModifyPlacableButton(Button btn, Type placable)
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
            }

            btn.enabledFunction = Enabled;
            btn.mouseDownLeft += Buy;
        }

        public override void Activate()
        { }
    }
}
