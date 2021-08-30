using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    /// <remarks/>
    [Serializable]
    public class RangeAssignmentDto : AssignmentBaseDto
    {
        /// <remarks/>
        [XmlAttribute("faceIndexBegin")]
        public int FaceIndexBegin { get; set; }

        /// <remarks/>
        [XmlAttribute("faceIndexEnd")]
        public int FaceIndexEnd { get; set; }
    }
}