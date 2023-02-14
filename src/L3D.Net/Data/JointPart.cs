using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.Data;

internal class JointPart : TransformablePart
{
    public JointPart(string name) : base(name)
    {
    }

    public List<GeometryPart> Geometries { get; } = new List<GeometryPart>();
    public Vector3? DefaultRotation { get; set; }
    public AxisRotation XAxis { get; set; }
    public AxisRotation YAxis { get; set; }
    public AxisRotation ZAxis { get; set; }
}