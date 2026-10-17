using ChaosFramework.Collections;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Input;
using ChaosFramework.Input.InputEvents;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using ChaosFramework.Platform;
using SysCol = System.Collections.Generic;

namespace ChaosTemplates.Game3D.Menu
{
    public abstract class Menu<T>
        : DialogScene where T : Menu<T>
    {
        public readonly struct ButtonHandler(string id, Control.Action handler)
        {
            public readonly string id = id;
            public readonly Control.Action handler = handler;
        }

        class Entry(ButtonHandler descr)
        {
            public readonly ButtonHandler descr = descr;
            public LinkedList<InputAxis> inputs = [];
            public Button btn;
        }

        public const float SZ = 0.025f;
        public static readonly Vector2f MARGIN = new(0.025f, 0.0666f);

        protected readonly Game g;
        readonly LinkedList<Entry> buttons = [];
        readonly LinkedList<InputDevice> devices = [];
        readonly string layoutFile;
        readonly string layoutPath;

        public Menu(
            MenuContext context,
            string layoutFile,
            string layoutPath
            ) : base(context, new Vector2f(context.window.Ratio(), 1), false)
        {
            this.layoutFile = layoutFile;
            this.layoutPath = layoutPath;
            g = (Game)game;

            foreach (ButtonHandler btn in MakeButtons())
                buttons.Add(new Entry(btn));

            Build();
        }

        protected abstract SysCol.IEnumerable<ButtonHandler> MakeButtons();
        protected abstract TextureContainer.Entry GetTexture();

        void Build()
        {
            panel.DisposeChildren();
            foreach (Entry e in buttons)
                e.inputs.Clear();

            devices.Clear();

            BuildMenu(layoutFile, layoutPath, null, null);
            panel.texture = GetTexture();

            foreach (Entry e in buttons)
            {
                e.btn = panel.GetControlById(e.descr.id) as Button;
                if (e.btn != null)
                    e.btn.mouseDownLeft += e.descr.handler;
            }
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();

            ((Game)game).input.context.AddHandler<InputPushEvent<Mouse.Button>, Mouse.Button, InputChange>(InputLayer.Menu, e =>
            {
                foreach (Entry btn in buttons)
                    if (btn.inputs.Contains(e.axis))
                    {
                        btn.btn?.mouseDownLeft?.DynamicInvoke(btn.btn);
                        return true;
                    }

                return false;
            });
        }
    }
}
