using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    public abstract class GeometryDefinitionDto
    {
        /// <remarks/>
        [XmlAttribute("id")]
        public string Id { get; set; }
    }
}