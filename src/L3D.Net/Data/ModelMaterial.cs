using System;
using System.Numerics;

namespace L3D.Net.Data
{
    internal class ModelMaterial
    {
        public string Name { get; }
        public Vector3 Color { get; }
        public string TextureName { get; }

        public ModelMaterial(string name, Vector3 color, string textureName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(name));
            
            Name = name;
            Color = color;
            TextureName = textureName;
        }
    }
}