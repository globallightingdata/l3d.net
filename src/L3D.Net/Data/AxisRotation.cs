using System;

namespace L3D.Net.Data
{
    internal class AxisRotation
    {
        public AxisRotation(double min, double max, double step)
        {
            if (max <= min)
                throw new ArgumentException($"Max value ({max}) needs to be greater than min value ({min})!");

            if (step <= 0)
                throw new ArgumentException("Step value needs to be positive!");

            Min = min;
            Max = max;
            Step = step;
        }

        public double Min { get; }
        public double Max { get; }
        public double Step { get; }
    }
}