using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_11_0.Dto;

/// <remarks/>
[Serializable]
public class LightEmittingPartDto : TransformablePartDto
{
    /// <remarks/>
    [XmlElement(ElementName = "Rectangle", Type = typeof(RectangleDto))]
    [XmlElement(ElementName = "Circle", Type = typeof(CircleDto))]
    public ShapeDto? Shape { get; set; }

    /// <remarks/>
    public LuminousHeightsDto? LuminousHeights { get; set; }
}