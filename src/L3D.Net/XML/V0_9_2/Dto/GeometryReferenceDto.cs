using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_2.Dto
{
    [Serializable]
    public class GeometryReferenceDto : GeometrySourceDto
    {
        /// <remarks/>
        [XmlAttribute("geometryId")]
        public string GeometryId { get; set; }
    }
}