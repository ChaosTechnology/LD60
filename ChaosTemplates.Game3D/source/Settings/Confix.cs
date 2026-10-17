using System.IO;
using ChaosUtil.Serialization.Text;

namespace ChaosTemplates.Game3D
{
    public static class Confix
    {
        public static readonly string settingsFile = "./settings.ini";

        public static Settings.ConfixClass instance = new();

        public static void Load()
        {
            if (File.Exists(settingsFile))
                Ini.Load(ref instance, settingsFile);
        }

        public static void Save() => Ini.Save(ref instance, settingsFile);

        public static void RestoreDefaults() => instance = new Settings.ConfixClass();
    }
}
