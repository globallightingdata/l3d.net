using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    /// <remarks/>
    [Serializable]
    [XmlInclude(typeof(TransformableNodeDto))]
    [XmlInclude(typeof(LightEmittingNodeDto))]
    [XmlInclude(typeof(JointNodeDto))]
    [XmlInclude(typeof(GeometryNodeDto))]
    public class NodeDto
    {
        /// <remarks/>
        [XmlAttribute("partName")]
        public string PartName { get; set; }
    }
}