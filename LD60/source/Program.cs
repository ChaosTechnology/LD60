using System;
using System.Reflection;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.Text;
using ChaosFramework.Input;
using ChaosFramework.IO;
using ChaosFramework.Platform;
using ChaosFramework.Platform.Glfw;
using ChaosUtil.Reflection;
using BindingFlags = System.Reflection.BindingFlags;

namespace LD60
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            typeof(System.Globalization.CultureInfo)
                .GetField("s_userDefaultCulture", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, System.Globalization.CultureInfo.InvariantCulture);

            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

#if OS_WINDOWS
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
#endif

            AssemblyManager.RegisterAssemblies(
                typeof(Program).Assembly,
                typeof(ChaosFramework.Menu.OpenGl.Control).Assembly,
                typeof(GlStateTracker.RenderStateChange).Assembly,
                typeof(ChaosFramework.Sound.Markers.CuePoint).Assembly  // TODO: Only register subtypes of CuePoint
                );

            ChaosIO.Init(
                typeof(Program).Assembly,
                typeof(ChaosFramework.Math.Matrix).Assembly,
                typeof(GlyphDimensions).Assembly,
                typeof(Graphics).Assembly
                );

            Confix.Load();

            string title = "LD60";

            GlfwPlatformContext platformContext = new();
            PresentationContext window = platformContext.CreateFullscreen(title, platformContext.PrimaryMonitor);
            Func<InputContext, InputDeviceHost> createHost = context => new ChaosFramework.Input.OpenTk.DeviceHost(context, ((GlfwFullscreen)window).window);

            window.SetIcon(
                new ApplicationIcon(
                    ApplicationIcon.IconFormat.ico,
                    () => Properties.Resources.ResourceManager.GetStream(nameof(Properties.Resources.icon)))
                    );

            Confix.Save();

            Game g = new(platformContext, window, createHost);
            g.Run();
        }
    }
}
