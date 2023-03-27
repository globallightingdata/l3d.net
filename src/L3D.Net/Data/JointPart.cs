using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.Data;

public class JointPart : TransformablePart
{
    public List<GeometryPart> Geometries { get; set; } = new();
    public Vector3? DefaultRotation { get; set; }
    public AxisRotation XAxis { get; set; } = new();
    public AxisRotation YAxis { get; set; } = new();
    public AxisRotation ZAxis { get; set; } = new();
}