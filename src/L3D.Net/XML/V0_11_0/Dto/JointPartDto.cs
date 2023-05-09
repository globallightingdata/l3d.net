using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_11_0.Dto;

/// <remarks/>
[Serializable]
public class JointPartDto : TransformablePartDto
{
    /// <remarks/>
    //[XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AxisRotationDto? XAxis { get; set; }

    /// <remarks/>
    //[XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AxisRotationDto? YAxis { get; set; }

    /// <remarks/>
    //[XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AxisRotationDto? ZAxis { get; set; }

    /// <remarks/>
    //[XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Vector3Dto? DefaultRotation { get; set; }

    /// <remarks/>
    //[XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Geometry", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    public List<GeometryPartDto> Geometries { get; set; } = new();
}