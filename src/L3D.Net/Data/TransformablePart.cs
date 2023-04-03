using System.Numerics;

namespace L3D.Net.Data;

public abstract class TransformablePart : Part
{
    public Vector3 Position { get; set; }

    public Vector3 Rotation { get; set; }
}