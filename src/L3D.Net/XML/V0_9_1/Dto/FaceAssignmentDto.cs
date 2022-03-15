using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_1.Dto
{
    /// <remarks/>
    [Serializable]
    public class FaceAssignmentDto : FaceAssignmentBaseDto
    {
        /// <remarks/>
        [XmlAttribute("faceIndex")]
        public int FaceIndex { get; set; }
    }
}