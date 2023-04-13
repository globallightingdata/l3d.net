using System.Xml.Serialization;

namespace L3D.Net.XML.V0_11_0.Dto;

public class GeometryFileDefinitionDto : GeometryDefinitionDto
{
    /// <remarks/>
    [XmlAttribute("filename")]
    public string FileName { get; set; } = string.Empty;

    /// <remarks/>
    [XmlAttribute("units")]
    public GeometricUnitsDto Units { get; set; }
}