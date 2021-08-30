using System;

namespace L3D.Net.Data
{
    internal class Circle : Shape
    {
        public Circle(double diameter)
        {
            if (diameter <= 0)
                throw new ArgumentException("Diameter must be positive!");

            Diameter = diameter;
        }

        public double Diameter { get; }
    }
}