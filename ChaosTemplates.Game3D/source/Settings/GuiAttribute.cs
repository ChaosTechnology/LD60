using System;

namespace ChaosTemplates.Game3D.Settings
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GuiAttribute(string page, Type controlProviderType, params object[] controlProviderArgs) : Attribute
    {
        public readonly string page = page;
        public readonly Type controlProviderType = controlProviderType;
        public readonly object[] controlProviderArgs = controlProviderArgs;
    }
}
