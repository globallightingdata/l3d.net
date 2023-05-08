using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Mapper.V0_11_0;

public class AxisRotationMapper : DtoMapperBase<AxisRotation, AxisRotationDto>
{
    public static readonly AxisRotationMapper Instance = new();

    protected override AxisRotation ConvertData(AxisRotationDto element) => new()
    {
        Min = element.Min,
        Max = element.Max,
        Step = element.Step
    };

    protected override AxisRotationDto ConvertData(AxisRotation element) => new()
    {
        Min = element.Min,
        Max = element.Max,
        Step = element.Step
    };
}