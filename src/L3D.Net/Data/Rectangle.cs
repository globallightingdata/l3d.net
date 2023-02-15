using System;

namespace L3D.Net.Data;

internal class Rectangle : Shape
{
    public Rectangle(double sizeX, double sizeY)
    {
        if (sizeX <= 0)
            throw new ArgumentException("sizeX must be positive!");
            
        if (sizeY <= 0)
            throw new ArgumentException("sizeY must be positive!");

        SizeX = sizeX;
        SizeY = sizeY;
    }

    public double SizeX { get; }
    public double SizeY { get; }
}