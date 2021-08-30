using System;

namespace L3D.Net.Data
{
    internal abstract class Part
    {
        protected Part(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Name = name;
        }

        public string Name { get; }
    }
}