using System;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using LD60.World.Tactician.Placables;

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
    }
}
