using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_11_0.Dto;

[Serializable]
public class GeometryReferenceDto
{
    /// <remarks/>
    [XmlAttribute("geometryId")]
    public string GeometryId { get; set; } = string.Empty;
}