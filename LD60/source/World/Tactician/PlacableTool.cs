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
        protected static readonly ImmutableArray<PlacableMeta> placables = PlacableMeta.GetTypes(typeof(Placable));

        static bool NotAbstract(Type t)
            => !t.IsAbstract;

        public override sealed void Select()
        {
            base.Select();
            CreatePlacableButtons(ModifyPlacableButton);
        }

        protected abstract void ModifyPlacableButton(Button btn, Type placable);

        protected void CreatePlacableButtons(Action<Button, Type> modifyButton)
        {
            scene.overlay.subTools.CreateControl<Image>(0, 0.05f).textureSource = string.Empty;
            foreach(Type placable in AssemblyManager.SubTypesOf<Placable>().Where(NotAbstract))
            {
                Button btn = scene.overlay.subTools.CreateControl<Button>(0, 0.05f, placable.Name);
                modifyButton?.Invoke(btn, placable);
            }
            scene.overlay.panel.Layout();
        }

    }
}
