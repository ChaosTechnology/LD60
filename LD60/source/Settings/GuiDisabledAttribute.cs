using System;

namespace LD60.Settings
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class GuiDisabledAttribute(string settingName, object value) : Attribute
    {
        public readonly string settingName = settingName;
        public readonly object value = value;
    }
}
