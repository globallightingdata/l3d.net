using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    public class GeometryFileDefinitionDto : GeometryDefinitionDto
    {
        /// <remarks/>
        [XmlAttribute("filename")]
        public string Filename { get; set; }

        /// <remarks/>
        [XmlAttribute("units")]
        public GeometryNodeUnits Units { get; set; }
    }
}