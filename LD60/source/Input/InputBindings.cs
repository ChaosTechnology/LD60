using System.Collections.Generic;
using ChaosFramework.Input;

namespace LD60.Input
{
    public class InputBindings(Input input)
    {
        readonly HashSet<InputAxis>[] inputs = new HashSet<InputAxis>[ChaosUtil.Reflection.Enum<Inputs>.GetValues().Length];

        public void UpdateInputBindings()
        {
            for (int i = 0; i < inputs.Length; ++i)
                inputs[i] = [];

            foreach (Keyboard keyboard in input.context.EnumerateDevices<Keyboard>())
            {
                inputs[(int)Inputs.Pause].Add(keyboard[Keyboard.HidUsage.Escape]);

                // walk with WASD
                inputs[(int)Inputs.PanForward].Add(keyboard[Keyboard.HidUsage.W]);
                inputs[(int)Inputs.PanLeft].Add(keyboard[Keyboard.HidUsage.A]);
                inputs[(int)Inputs.PanBack].Add(keyboard[Keyboard.HidUsage.S]);
                inputs[(int)Inputs.PanRight].Add(keyboard[Keyboard.HidUsage.D]);
                inputs[(int)Inputs.TurnCamCw].Add(keyboard[Keyboard.HidUsage.Q]);
                inputs[(int)Inputs.TurnCamCcw].Add(keyboard[Keyboard.HidUsage.E]);

                // walk with arrow keys
                inputs[(int)Inputs.PanForward].Add(keyboard[Keyboard.HidUsage.ArrowUp]);
                inputs[(int)Inputs.PanLeft].Add(keyboard[Keyboard.HidUsage.ArrowLeft]);
                inputs[(int)Inputs.PanBack].Add(keyboard[Keyboard.HidUsage.ArrowDown]);
                inputs[(int)Inputs.PanRight].Add(keyboard[Keyboard.HidUsage.ArrowRight]);
            }

            foreach(Mouse mouse in input.context.EnumerateDevices<Mouse>())
            {
                inputs[(int)Inputs.ActivateTool].Add(mouse.buttons[(int)Mouse.ButtonSemantic.Left]);
            }
        }

        public bool IsMatch(Inputs input, InputAxis axis)
            => inputs[(int)input].Contains(axis);

        public IEnumerable<InputAxis> GetBindings(Inputs input)
            => inputs[(int)input];
    }
}
