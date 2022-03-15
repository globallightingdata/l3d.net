using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_1.Dto
{
    /// <remarks/>
    [Serializable()]
    //[XmlType(AnonymousType = true)]
    public class RectangleDto : ShapeDto
    {
        /// <remarks/>
        [XmlAttribute("sizeX")]
        public double SizeX { get; set; }

        /// <remarks/>
        [XmlAttribute("sizeY")]
        public double SizeY { get; set; }
    }
}