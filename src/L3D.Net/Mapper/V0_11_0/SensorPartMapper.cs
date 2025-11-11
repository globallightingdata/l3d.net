using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Mapper.V0_11_0;

public sealed class SensorPartMapper : DtoMapperBase<SensorPart, SensorPartDto>
{
    public static readonly SensorPartMapper Instance = new();

    protected override SensorPart ConvertData(SensorPartDto element) => new()
    {
        Name = element.Name,
        Position = Vector3DMapper.Instance.Convert(element.Position),
        Rotation = Vector3DMapper.Instance.Convert(element.Rotation)
    };

    protected override SensorPartDto ConvertData(SensorPart element) => new()
    {
        Name = element.Name,
        Position = Vector3DMapper.Instance.Convert(element.Position),
        Rotation = Vector3DMapper.Instance.Convert(element.Rotation)
    };
}