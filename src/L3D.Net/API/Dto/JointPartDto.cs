using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.API.Dto;

public class JointPartDto : PartDto
{
    public Vector3? DefaultRotation { get; set; }
    public AxisRotationDto XAxis { get; set; }
    public AxisRotationDto YAxis { get; set; }
    public AxisRotationDto ZAxis { get; set; }

    public List<GeometryPartDto> Geometries { get; set; }
}