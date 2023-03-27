using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.Data;

public class GeometryPart : TransformablePart
{
    public GeometrySource GeometrySource { get; set; } = new();
    public List<JointPart> Joints { get; set; } = new();
    public bool IncludedInMeasurement { get; set; } = true;
    public List<Vector3> ElectricalConnectors { get; set; } = new();
    public List<Vector3> PendulumConnectors { get; set; } = new();
    public List<LightEmittingPart> LightEmittingObjects { get; set; } = new();
    public List<LightEmittingSurfacePart> LightEmittingSurfaces { get; set; } = new();
    public List<SensorPart> Sensors { get; set; } = new();
}