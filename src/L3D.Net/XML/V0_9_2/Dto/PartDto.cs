using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_2.Dto;

/// <remarks/>
[Serializable]
[XmlInclude(typeof(TransformablePartDto))]
[XmlInclude(typeof(LightEmittingPartDto))]
[XmlInclude(typeof(JointPartDto))]
[XmlInclude(typeof(GeometryPartDto))]
public class PartDto
{
    /// <remarks/>
    [XmlAttribute("partName")]
    public string Name { get; set; }
}