using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Input;
using ChaosFramework.Input.InputEvents;
using ChaosFramework.IO.Containers;
using ChaosFramework.Menu.OpenGl;
using LD60.Input;
using SysCol = System.Collections.Generic;

namespace LD60.Menu
{
    public class PauseMenu(World.TemplateWorld world)
        : Menu<PauseMenu>(world.game.menuContext, "pausemenu_layout", "Root/Pause")
    {
        readonly World.TemplateWorld world = world;

        bool closeRequested = false;

        protected override AssetContainer<Texture>.Entry GetTexture() => null;

        protected override SysCol.IEnumerable<ButtonHandler> MakeButtons()
        {
            yield return new ButtonHandler("btn_Continue", KindlyRequestSuicide);
            yield return new ButtonHandler("btn_MainMenu", MainMenu);
            yield return new ButtonHandler("btn_Exit", Exit);
        }

        void MainMenu(Control _)
        {
            MainMenu menu = new(context);
            game.scenes.Add(menu);
            world.Dispose();
            Dispose();
        }

        void Exit(Control _)
            => ((Game)game).window.Close();

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();

            world.game.input.AddKeyDownHandler(InputLayer.Menu, Input);

            if (closeRequested)
            {
                closeRequested = false;
                KindlyRequestSuicide();
            }
        }

        bool Input(InputPushEvent<Keyboard.Key> e)
        {
            if (((Game)game).input.bindings.IsMatch(Inputs.Pause, e.axis))
                return closeRequested = true;

            return false;
        }
    }
}
