using System.Collections.Generic;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_10_0.Dto;

public class LightEmittingSurfacePartDto : PartDto
{
    /// <remarks/>
    [XmlElement("LightEmittingObjectReference", Type = typeof(LightEmittingObjectReferenceDto))]
    public List<LightEmittingObjectReferenceDto> LightEmittingPartIntensityMapping { get; set; } = new();

    /// <remarks/>
    [XmlArrayItem("FaceAssignment", Type = typeof(SingleFaceAssignmentDto))]
    [XmlArrayItem("FaceRangeAssignment", Type = typeof(FaceRangeAssignmentDto))]
    public List<FaceAssignmentDto> FaceAssignments { get; set; } = new();
}