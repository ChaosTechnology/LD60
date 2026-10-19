using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Menu.OpenGl;
using SysCol = System.Collections.Generic;

namespace LD60.Menu
{
    public class MainMenu(MenuContext context)
        : Menu<MainMenu>(context, "mainmenu_layout", "Root/Main")
    {
        protected override TextureContainer.Entry GetTexture()
            => g.textures.Load("textures/menu/mainmenu/background.png", this);

        protected override SysCol.IEnumerable<ButtonHandler> MakeButtons()
        {
            yield return new ButtonHandler("btn_Play", Play);
            yield return new ButtonHandler("btn_Settings", Settings);
            yield return new ButtonHandler("btn_Exit", Exit);
        }

        void Play(Control _)
        {
            Dispose();
            game.scenes.Add(new World.FloatingIslands(g));
        }

        void Settings(Control _)
            => new Settings(this).ShowDialog(null, null);

        void Exit(Control _)
            => ((Game)game).window.Close();
    }
}
