using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_10_0.Dto;

/// <remarks/>
[Serializable]
[XmlInclude(typeof(LightEmittingPartDto))]
[XmlInclude(typeof(JointPartDto))]
[XmlInclude(typeof(GeometryPartDto))]
public class TransformablePartDto : PartDto
{
    /// <remarks/>
    public Vector3Dto Position { get; set; } = new();

    /// <remarks/>
    public Vector3Dto Rotation { get; set; } = new();
}