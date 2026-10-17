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
                inputs[(int)Inputs.WalkForward].Add(keyboard[Keyboard.HidUsage.W]);
                inputs[(int)Inputs.WalkLeft].Add(keyboard[Keyboard.HidUsage.A]);
                inputs[(int)Inputs.WalkBack].Add(keyboard[Keyboard.HidUsage.S]);
                inputs[(int)Inputs.WalkRight].Add(keyboard[Keyboard.HidUsage.D]);

                // walk with arrow keys
                inputs[(int)Inputs.WalkForward].Add(keyboard[Keyboard.HidUsage.ArrowUp]);
                inputs[(int)Inputs.WalkLeft].Add(keyboard[Keyboard.HidUsage.ArrowLeft]);
                inputs[(int)Inputs.WalkBack].Add(keyboard[Keyboard.HidUsage.ArrowDown]);
                inputs[(int)Inputs.WalkRight].Add(keyboard[Keyboard.HidUsage.ArrowRight]);
            }
        }

        public bool IsMatch(Inputs input, InputAxis axis)
            => inputs[(int)input].Contains(axis);

        public IEnumerable<InputAxis> GetBindings(Inputs input)
            => inputs[(int)input];
    }
}
