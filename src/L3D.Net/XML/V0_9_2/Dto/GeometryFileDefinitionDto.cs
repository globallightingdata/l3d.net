using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_2.Dto;

public class GeometryFileDefinitionDto : GeometryDefinitionDto
{
    /// <remarks/>
    [XmlAttribute("filename")]
    public string FileName { get; set; }

    /// <remarks/>
    [XmlAttribute("units")]
    public GeometricUnitsDto Units { get; set; }
}