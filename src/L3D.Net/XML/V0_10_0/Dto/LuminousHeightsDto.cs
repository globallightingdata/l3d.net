using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_10_0.Dto;

[Serializable]
public class LuminousHeightsDto
{
    [XmlAttribute("c0")]
    public double C0 { get; set; }

    [XmlAttribute("c90")]
    public double C90 { get; set; }

    [XmlAttribute("c180")]
    public double C180 { get; set; }

    [XmlAttribute("c270")]
    public double C270 { get; set; }
}