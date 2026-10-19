using ChaosFramework.Components;
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
