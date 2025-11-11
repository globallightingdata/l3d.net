using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;
using System.Linq;

namespace L3D.Net.Mapper.V0_11_0;

public sealed class JointPartMapper : DtoMapperBase<JointPart, JointPartDto>
{
    public static readonly JointPartMapper Instance = new();

    protected override JointPart ConvertData(JointPartDto element) => new()
    {
        Name = element.Name,
        DefaultRotation = element.DefaultRotation == null ? null : Vector3DMapper.Instance.Convert(element.DefaultRotation),
        Position = Vector3DMapper.Instance.Convert(element.Position),
        Rotation = Vector3DMapper.Instance.Convert(element.Rotation),
        XAxis = AxisRotationMapper.Instance.ConvertNullable(element.XAxis),
        YAxis = AxisRotationMapper.Instance.ConvertNullable(element.YAxis),
        ZAxis = AxisRotationMapper.Instance.ConvertNullable(element.ZAxis),
        Geometries = element.Geometries.Select(GeometryPartMapper.Instance.Convert).ToList()
    };

    protected override JointPartDto ConvertData(JointPart element) => new()
    {
        Name = element.Name,
        DefaultRotation = element.DefaultRotation == null ? null : Vector3DMapper.Instance.Convert(element.DefaultRotation.Value),
        Position = Vector3DMapper.Instance.Convert(element.Position),
        Rotation = Vector3DMapper.Instance.Convert(element.Rotation),
        XAxis = AxisRotationMapper.Instance.ConvertNullable(element.XAxis),
        YAxis = AxisRotationMapper.Instance.ConvertNullable(element.YAxis),
        ZAxis = AxisRotationMapper.Instance.ConvertNullable(element.ZAxis),
        Geometries = element.Geometries.Select(GeometryPartMapper.Instance.Convert).ToList(),
    };
}