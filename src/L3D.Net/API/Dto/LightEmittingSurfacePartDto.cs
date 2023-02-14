using System.Collections.Generic;

namespace L3D.Net.API.Dto;

public class LightEmittingSurfacePartDto : PartDto
{
    public Dictionary<string, double> LightEmittingObjects { get; set; }
    public List<BaseAssignmentDto> FaceAssignments { get; set; }
}