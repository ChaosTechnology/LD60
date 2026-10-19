using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl.Controls;

namespace LD60.Tactician
{
    using ToolInfo = (bool active, float cooldown);

    public class ToolButton
        : Button
    {
        static ToolInfo DefaultToolInfo()
            => (false, 0);

        public delegate ToolInfo GetToolInfo();

        public GetToolInfo getToolInfo = DefaultToolInfo;

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            shader = ((Game)scene.game).shaders.Load("shaders/toolbutton.fx", this);
        }

        public override void Draw()
        {
            ToolInfo toolButtonInfo = getToolInfo();
            shader.SetValue("info", new Vector3f(
                ftime.totalTime,
                toolButtonInfo.cooldown,
                toolButtonInfo.active ? 1 : 0
                ));
            base.Draw();
        }
    }
}
