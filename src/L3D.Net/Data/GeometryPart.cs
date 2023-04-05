using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.Data;

public class GeometryPart : TransformablePart
{
    public GeometryFileDefinition GeometryReference { get; set; } = new();
    public List<JointPart>? Joints { get; set; }
    public bool IncludedInMeasurement { get; set; } = true;
    public List<Vector3>? ElectricalConnectors { get; set; }
    public List<Vector3>? PendulumConnectors { get; set; }
    public List<LightEmittingPart>? LightEmittingObjects { get; set; }
    public List<LightEmittingSurfacePart>? LightEmittingSurfaces { get; set; }
    public List<SensorPart>? Sensors { get; set; }
}