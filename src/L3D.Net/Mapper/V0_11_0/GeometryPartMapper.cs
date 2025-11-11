using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;
using System.Linq;

namespace L3D.Net.Mapper.V0_11_0;

public sealed class GeometryPartMapper : DtoMapperBase<GeometryPart, GeometryPartDto>
{
    public static readonly GeometryPartMapper Instance = new();

    protected override GeometryPart ConvertData(GeometryPartDto element) => new()
    {
        Name = element.Name,
        ElectricalConnectors = element.ElectricalConnectors?.Select(x => Vector3DMapper.Instance.Convert(x)).ToList(),
        GeometryReference = GeometryReferenceMapper.Instance.Convert(element.GeometryReference),
        Position = Vector3DMapper.Instance.Convert(element.Position),
        Rotation = Vector3DMapper.Instance.Convert(element.Rotation),
        IncludedInMeasurement = element.IncludedInMeasurement,
        Joints = element.Joints?.Select(JointPartMapper.Instance.Convert).ToList(),
        LightEmittingObjects = element.LightEmittingObjects?.Select(LightEmittingPartMapper.Instance.Convert).ToList(),
        LightEmittingSurfaces = element.LightEmittingSurfaces?.Select(LightEmittingSurfacePartMapper.Instance.Convert).ToList(),
        PendulumConnectors = element.PendulumConnectors?.Select(Vector3DMapper.Instance.Convert).ToList(),
        Sensors = element.Sensors?.Select(SensorPartMapper.Instance.Convert).ToList()
    };

    protected override GeometryPartDto ConvertData(GeometryPart element) => new()
    {
        Name = element.Name,
        ElectricalConnectors = element.ElectricalConnectors?.Select(x => Vector3DMapper.Instance.Convert(x)).ToArray(),
        GeometryReference = GeometryReferenceMapper.Instance.Convert(element.GeometryReference),
        Position = Vector3DMapper.Instance.Convert(element.Position),
        Rotation = Vector3DMapper.Instance.Convert(element.Rotation),
        IncludedInMeasurement = element.IncludedInMeasurement,
        Joints = element.Joints?.Select(JointPartMapper.Instance.Convert).ToArray(),
        LightEmittingObjects = element.LightEmittingObjects?.Select(LightEmittingPartMapper.Instance.Convert).ToArray(),
        LightEmittingSurfaces = element.LightEmittingSurfaces?.Select(LightEmittingSurfacePartMapper.Instance.Convert).ToArray(),
        PendulumConnectors = element.PendulumConnectors?.Select(Vector3DMapper.Instance.Convert).ToArray(),
        Sensors = element.Sensors?.Select(SensorPartMapper.Instance.Convert).ToArray()
    };
}