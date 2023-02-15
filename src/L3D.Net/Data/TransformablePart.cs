using System.Numerics;

namespace L3D.Net.Data;

internal abstract class TransformablePart : Part
{
    protected TransformablePart(string name) : base(name)
    {
    }

    public Vector3 Position { get; set; }

    public Vector3 Rotation { get; set; }
}