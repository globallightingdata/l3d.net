using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_2.Dto;

public class LightEmittingObjectReferenceDto
{
    /// <remarks/>
    [XmlAttribute("lightEmittingPartName")]
    public string LightEmittingPartName { get; set; }

    /// <remarks/>
    [XmlAttribute("intensity")]
    public double Intensity { get; set; } = 1.0;

    [ExcludeFromCodeCoverage]
    public bool ShouldSerializeIntensity() => Math.Abs(Intensity - 1.0) > 0.0001;
}