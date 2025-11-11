using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Mapper.V0_11_0;

public sealed class LightEmittingPartMapper : DtoMapperBase<LightEmittingPart, LightEmittingPartDto>
{
    public static readonly LightEmittingPartMapper Instance = new();

    protected override LightEmittingPart ConvertData(LightEmittingPartDto element) => new(ShapeMapper.Instance.Convert(element.Shape))
    {
        Name = element.Name,
        Position = Vector3DMapper.Instance.Convert(element.Position),
        Rotation = Vector3DMapper.Instance.Convert(element.Rotation),
        LuminousHeights = LuminousHeightsMapper.Instance.ConvertNullable(element.LuminousHeights)
    };

    protected override LightEmittingPartDto ConvertData(LightEmittingPart element) => new(ShapeMapper.Instance.Convert(element.Shape))
    {
        Name = element.Name,
        Position = Vector3DMapper.Instance.Convert(element.Position),
        Rotation = Vector3DMapper.Instance.Convert(element.Rotation),
        LuminousHeights = LuminousHeightsMapper.Instance.ConvertNullable(element.LuminousHeights)
    };
}