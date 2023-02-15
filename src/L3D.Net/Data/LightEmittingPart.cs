using System;

namespace L3D.Net.Data;

internal class LightEmittingPart : TransformablePart
{
    public LightEmittingPart(string name, Shape shape) : base(name)
    {
        Shape = shape ?? throw new ArgumentNullException(nameof(shape));
    }

    public Shape Shape { get; }
    public LuminousHeights LuminousHeights { get; set; }
}