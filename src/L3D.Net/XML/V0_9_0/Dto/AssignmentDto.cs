using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    /// <remarks/>
    [Serializable]
    public class AssignmentDto : AssignmentBaseDto
    {
        /// <remarks/>
        [XmlAttribute("faceIndex")]
        public int FaceIndex { get; set; }
    }
}