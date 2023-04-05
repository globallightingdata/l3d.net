using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_10_0.Dto;

/// <remarks/>
[Serializable]
public class LightEmittingPartDto : TransformablePartDto
{
    /// <remarks/>
    [XmlElement(ElementName = "Rectangle", Type = typeof(RectangleDto))]
    [XmlElement(ElementName = "Circle", Type = typeof(CircleDto))]
    public ShapeDto Shape { get; set; }

    /// <remarks/>
    public LuminousHeightsDto? LuminousHeights { get; set; }

#pragma warning disable CS8618
    private LightEmittingPartDto()
#pragma warning restore CS8618
    {
    }

    public LightEmittingPartDto(ShapeDto shape)
    {
        Shape = shape;
    }
}