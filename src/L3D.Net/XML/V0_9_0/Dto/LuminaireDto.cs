using System.Collections.Generic;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    /// <remarks/>
    [XmlRoot("Luminaire", Namespace = "", IsNullable = false)]
    public class LuminaireDto
    {
        [XmlAttribute("noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Scheme = Constants.CurrentSchemeUri;

        /// <remarks/>
        public HeaderDto Header { get; set; }

        /// <remarks/>
        [XmlArrayItem("GeometryFileDefinition", Type = typeof(GeometryFileDefinitionDto))]
        public List<GeometryDefinitionDto> GeometryDefinitions { get; set; }

        /// <remarks/>
        [XmlArray("Structure")]
        [XmlArrayItem("Geometry")]
        public List<GeometryNodeDto> Parts { get; set; }
    }
}
