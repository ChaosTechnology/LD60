using System;

namespace LD60.World.Tactician
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ToolAttribute(string mesh, string material)
        : Attribute
    {
        public readonly string mesh = mesh;
        public readonly string material = material;
    }
}
