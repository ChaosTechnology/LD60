using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.IO.Containers;
using ChaosFramework.Menu.OpenGl;
using SysCol = System.Collections.Generic;

namespace LD60.Menu
{
    public class Death(World.FloatingIslands world)
        : Menu<Death>(world.game.menuContext, "death_layout", "Root/Death")
    {
        readonly World.FloatingIslands world = world;

        protected override AssetContainer<Texture>.Entry GetTexture() => null;

        protected override SysCol.IEnumerable<ButtonHandler> MakeButtons()
        {
            yield return new ButtonHandler("btn_TryAgain", TryAgain);
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

        void TryAgain(Control _)
        {
            Dispose();
            world.Dispose();
            game.scenes.Add(new World.FloatingIslands(world.game));
        }
    }
}
