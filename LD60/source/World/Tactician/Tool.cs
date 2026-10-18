using System;
using System.Linq;
using ChaosFramework.Components;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosUtil.Reflection;

namespace LD60.World.Tactician
{
    [AssemblyManager.ListSubTypes]
    public abstract class Tool
        : Component<FloatingIslands>
    {
        static bool NotAbstract(Type t)
            => !t.IsAbstract;

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
            foreach(Type placable in AssemblyManager.SubTypesOf<Placable>().Where(NotAbstract))
            {
                Button btn = scene.overlay.subTools.CreateControl<Button>(0, 0.05f, placable.Name);
                modifyButton?.Invoke(btn, placable);
            }
            scene.overlay.panel.Layout();
        }

        public abstract void Activate();

        public override sealed void SetUpdateCalls()
        {
            if (scene.overlay.currentTool == this)
            {
                base.SetUpdateCalls();
                SetUpdateCalls2();
            }
        }

        public override sealed void SetDrawCalls()
        {
            if (scene.overlay.currentTool == this)
            {
                base.SetDrawCalls();
                SetDrawCalls2();
            }
        }

        protected virtual void SetUpdateCalls2()
        {}


        protected virtual void SetDrawCalls2()
        {}
    }
}
