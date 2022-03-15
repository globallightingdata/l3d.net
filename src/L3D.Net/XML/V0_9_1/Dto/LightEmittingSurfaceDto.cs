using System.Collections.Generic;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_1.Dto
{
    public class LightEmittingSurfaceDto : NodeDto
    {
        /// <remarks/>
        [XmlElement("LightEmittingObjectReference", Type = typeof(LightEmittingObjectReferenceDto))]
        public List<LightEmittingObjectReferenceDto> LightEmittingObjectReference { get; set; }

        /// <remarks/>
        [XmlArrayItem("FaceAssignment", Type = typeof(FaceAssignmentDto))]
        [XmlArrayItem("FaceRangeAssignment", Type = typeof(FaceRangeAssignmentDto))]
        public List<FaceAssignmentBaseDto> FaceAssignments { get; set; }
    }
}