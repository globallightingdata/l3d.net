using System.Collections.Generic;

namespace L3D.Net.Data;

public class LightEmittingSurfacePart : Part
{
    public Dictionary<string, double> LightEmittingPartIntensityMapping { get; set; } = new();

    public List<FaceAssignment> FaceAssignments { get; set; } = new();
}