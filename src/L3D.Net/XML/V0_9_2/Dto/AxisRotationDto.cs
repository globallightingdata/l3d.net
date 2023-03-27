using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_2.Dto;

/// <remarks/>
[Serializable]
public class AxisRotationDto
{
    /// <remarks/>
    [XmlAttribute("min")]
    public double Min { get; set; }

    /// <remarks/>
    [XmlAttribute("max")]
    public double Max { get; set; }

    /// <remarks/>
    [XmlAttribute("step")]
    public double Step { get; set; }
}