using System;
using System.Linq;
using ChaosFramework.Components;
using ChaosFramework.Core;
using ChaosFramework.Input;
using ChaosFramework.Input.Axis;
using ChaosFramework.Input.InputEvents;

namespace ChaosTemplates.Game3D.Input
{
    public class Input
        : Disposable
    {
        const float UPDATE_INPUT_DEVICE_INTERVAL = 3.2f;
        public InputContext context { get; private set; }

        public readonly InputBindings bindings;

        readonly Func<InputContext, InputDeviceHost> createInputContext;
        float updateInputDeviceTimer;

        public Input(Func<InputContext, InputDeviceHost> createInputContext)
        {
            this.createInputContext = createInputContext;
            bindings = new InputBindings(this);
        }

        public void Init()
        {
            context = new InputContext(typeof(InputLayer), createInputContext);
            UpdateDevices();
        }

        void UpdateDevices()
        {
            context.UpdateDeviceList();
            updateInputDeviceTimer = UPDATE_INPUT_DEVICE_INTERVAL;
            bindings.UpdateInputBindings();
        }

        public void UpdateDeviceRefresh(Time ftime)
        {
            updateInputDeviceTimer -= ftime;
            if (updateInputDeviceTimer < 0)
                UpdateDevices();
        }

        public bool IsPressed(Inputs input)
            => bindings.GetBindings(input).OfType<ButtonAxis>().Any(axis => axis.down);

        public float MouseX()
            => context.EnumerateDevices<Mouse>().Sum(mouse => mouse.x.value) * Confix.instance.mouseSensitivity;

        public float MouseY()
            => context.EnumerateDevices<Mouse>().Sum(mouse => mouse.y.value) * Confix.instance.mouseSensitivity;

        public float Scroll()
            => context.EnumerateDevices<Mouse>().Sum(mouse => mouse.scroll.value);

        public void AddMouseMoveHandler(InputLayer layer, Func<InputChangeEvent<Mouse.Position>, bool> handler)
            => context.AddHandler<InputChangeEvent<Mouse.Position>, Mouse.Position, InputChange>(layer, handler);

        public void AddMouseDownHandler(InputLayer layer, Func<InputPushEvent<Mouse.Button>, bool> handler)
            => context.AddHandler<InputPushEvent<Mouse.Button>, Mouse.Button, InputChange>(layer, handler);

        public void AddMouseUpHandler(InputLayer layer, Func<InputReleaseEvent<Mouse.Button>, bool> handler)
            => context.AddHandler<InputReleaseEvent<Mouse.Button>, Mouse.Button, InputChange>(layer, handler);

        public void AddKeyDownHandler(InputLayer layer, Func<InputPushEvent<Keyboard.Key>, bool> handler)
            => context.AddHandler<InputPushEvent<Keyboard.Key>, Keyboard.Key, InputChange>(layer, handler);

        public void AddKeyUpHandler(InputLayer layer, Func<InputReleaseEvent<Keyboard.Key>, bool> handler)
            => context.AddHandler<InputReleaseEvent<Keyboard.Key>, Keyboard.Key, InputChange>(layer, handler);

        protected override void DoDispose()
        {
            base.DoDispose();
            context.Dispose();
        }
    }
}
