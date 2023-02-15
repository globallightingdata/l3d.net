using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_2.Dto;

/// <remarks/>
[Serializable]
public class LightEmittingNodeDto : TransformableNodeDto
{
    /// <remarks/>
    [XmlElement(ElementName = "Rectangle", Type = typeof(RectangleDto))]
    [XmlElement(ElementName = "Circle", Type = typeof(CircleDto))]
    public ShapeDto Shape { get; set; }
        
    /// <remarks/>
    public LuminousHeightsDto LuminousHeights { get; set; }
}