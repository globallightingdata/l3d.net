using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_10_0.Dto;

/// <remarks/>
[Serializable]
[DebuggerDisplay("({X} | {Y} | {Z})")]
public class Vector3Dto
{
    /// <remarks/>
    [XmlAttribute("x")]
    public float X { get; set; }

    /// <remarks/>
    [XmlAttribute("y")]
    public float Y { get; set; }

    /// <remarks/>
    [XmlAttribute("z")]
    public float Z { get; set; }
}