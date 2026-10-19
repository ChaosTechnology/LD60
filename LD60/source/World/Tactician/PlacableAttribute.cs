using System;

namespace LD60.World.Tactician
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PlacableAttribute(string mesh, string material, int gold)
        : Attribute
    {
        public readonly int gold = gold;
        public readonly string mesh = mesh;
        public readonly string material = material;
    }
}
