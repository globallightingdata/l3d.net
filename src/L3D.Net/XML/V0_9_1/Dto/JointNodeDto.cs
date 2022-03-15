using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_1.Dto
{
    /// <remarks/>
    [Serializable]
    public class JointNodeDto : TransformableNodeDto
    {
        /// <remarks/>
        //[XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AxisRotationTypeDto XAxis { get; set; }

        /// <remarks/>
        //[XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AxisRotationTypeDto YAxis { get; set; }

        /// <remarks/>
        //[XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AxisRotationTypeDto ZAxis { get; set; }

        /// <remarks/>
        //[XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Vector3Dto DefaultRotation { get; set; }

        /// <remarks/>
        //[XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem("Geometry", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public List<GeometryNodeDto> Geometries { get; set; }
    }
}