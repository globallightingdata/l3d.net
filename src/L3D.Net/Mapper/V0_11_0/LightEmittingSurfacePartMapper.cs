using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;
using System.Linq;

namespace L3D.Net.Mapper.V0_11_0;

public sealed class LightEmittingSurfacePartMapper : DtoMapperBase<LightEmittingSurfacePart, LightEmittingSurfacePartDto>
{
    public static readonly LightEmittingSurfacePartMapper Instance = new();

    protected override LightEmittingSurfacePart ConvertData(LightEmittingSurfacePartDto element) => new()
    {
        Name = element.Name,
        FaceAssignments = element.FaceAssignments.Select(x => FaceAssignmentMapper.Instance.Convert(x)).ToList(),
        LightEmittingPartIntensityMapping = element.LightEmittingPartIntensityMapping.ToDictionary(x => x.LightEmittingPartName, x => x.Intensity)
    };

    protected override LightEmittingSurfacePartDto ConvertData(LightEmittingSurfacePart element) => new()
    {
        Name = element.Name,
        FaceAssignments = element.FaceAssignments.Select(x => FaceAssignmentMapper.Instance.Convert(x)).ToList(),
        LightEmittingPartIntensityMapping = element.LightEmittingPartIntensityMapping.Select(x => new LightEmittingObjectReferenceDto
        {
            LightEmittingPartName = x.Key,
            Intensity = x.Value
        }).ToList()
    };
}