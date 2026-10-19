using System;

namespace LD60.World.Tactician
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PlacableAttribute(string mesh, string material, int gold, float cooldown)
        : Attribute
    {
        public readonly int gold = gold;
        public readonly float cooldown = cooldown;
        public readonly string mesh = mesh;
        public readonly string material = material;
    }
}
