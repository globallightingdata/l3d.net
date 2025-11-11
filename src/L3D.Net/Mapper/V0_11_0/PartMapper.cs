using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;
using System;

namespace L3D.Net.Mapper.V0_11_0;

public sealed class PartMapper : DtoMapperBase<Part, PartDto>
{
    public static readonly PartMapper Instance = new();

    protected override Part ConvertData(PartDto element) => element switch
    {
        LightEmittingSurfacePartDto lightEmittingSurface => LightEmittingSurfacePartMapper.Instance.Convert(lightEmittingSurface),
        JointPartDto jointPart => JointPartMapper.Instance.Convert(jointPart),
        LightEmittingPartDto lightEmittingPart => LightEmittingPartMapper.Instance.Convert(lightEmittingPart),
        GeometryPartDto geometryPart => GeometryPartMapper.Instance.Convert(geometryPart),
        SensorPartDto sensorPart => SensorPartMapper.Instance.Convert(sensorPart),
        _ => throw new ArgumentOutOfRangeException(nameof(element))
    };

    protected override PartDto ConvertData(Part element) => element switch
    {
        LightEmittingSurfacePart lightEmittingSurface => LightEmittingSurfacePartMapper.Instance.Convert(lightEmittingSurface),
        JointPart jointPart => JointPartMapper.Instance.Convert(jointPart),
        LightEmittingPart lightEmittingPart => LightEmittingPartMapper.Instance.Convert(lightEmittingPart),
        GeometryPart geometryPart => GeometryPartMapper.Instance.Convert(geometryPart),
        SensorPart sensorPart => SensorPartMapper.Instance.Convert(sensorPart),
        _ => throw new ArgumentOutOfRangeException(nameof(element))
    };
}