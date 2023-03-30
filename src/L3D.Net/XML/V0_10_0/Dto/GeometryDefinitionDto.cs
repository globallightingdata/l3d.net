using System.Xml.Serialization;

namespace L3D.Net.XML.V0_10_0.Dto;

public abstract class GeometryDefinitionDto
{
    /// <remarks/>
    [XmlAttribute("id")]
    public string GeometryId { get; set; } = string.Empty;
}