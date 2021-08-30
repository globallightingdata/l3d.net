using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    /// <remarks/>
    [Serializable]
    [XmlInclude(typeof(LightEmittingNodeDto))]
    [XmlInclude(typeof(JointNodeDto))]
    [XmlInclude(typeof(GeometryNodeDto))]
    public class TransformableNodeDto : NodeDto
    {
        /// <remarks/>
        public Vector3Dto Position { get; set; }

        /// <remarks/>
        public Vector3Dto Rotation { get; set; }
    }
}