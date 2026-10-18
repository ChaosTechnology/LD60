using System;
using ChaosFramework.Components;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosUtil.Reflection;

namespace LD60.World.Tactician
{
    [AssemblyManager.ListSubTypes]
    public abstract class Tool
        : Component<FloatingIslands>
    {
        protected override void Create(CreateParameters cparams)
        {
        }

        public virtual void Select()
        {
            scene.overlay.subTools.DisposeChildren();
        }

        protected void CreatePlacableButtons(Action<Button, Type> modifyButton)
        {
            scene.overlay.subTools.CreateControl<Image>(0, 0.05f).textureSource = string.Empty;
            foreach(Type placable in AssemblyManager.SubTypesOf<Placable>())
            {
                Button btn = scene.overlay.subTools.CreateControl<Button>(0, 0.05f, placable.Name);
                modifyButton?.Invoke(btn, placable);
            }
            scene.overlay.panel.Layout();
        }

        public abstract void Activate();
    }
}
