using System;
using System.Linq;
using ChaosFramework.Collections.Immutable;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosUtil.Reflection;

namespace LD60.World.Tactician
{
    using PlacableMeta  = TypeWithAttribute<PlacableAttribute>;

    public abstract class PlacableTool
        : Tool
    {
        public static readonly ImmutableArray<PlacableMeta> placables = PlacableMeta.GetTypes(typeof(Placable));

        static bool NotAbstract(Type t)
            => !t.IsAbstract;

        public override void Select()
        {
            base.Select();
            CreatePlacableButtons();
        }

        protected void CreatePlacableButtons()
        {
            scene.overlay.subTools.CreateControl<Image>(0, 0.05f).textureSource = string.Empty;
            foreach(Type placable in AssemblyManager.SubTypesOf<Placable>().Where(NotAbstract))
            {
                Button btn = scene.overlay.subTools.CreateControl<Button>(0, 0.05f, placable.Name);
                ModifyPlacableButton(btn, placable);
            }
            scene.overlay.panel.Layout();
        }

        protected abstract void ModifyPlacableButton(Button btn, Type placable);

    }
}
