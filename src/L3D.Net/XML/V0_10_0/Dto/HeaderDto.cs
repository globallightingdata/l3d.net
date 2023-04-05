﻿using System;

namespace L3D.Net.XML.V0_10_0.Dto;

/// <remarks/>
//[Serializable]
public class HeaderDto
{
    /// <remarks/>
    //[XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Name { get; set; }

    /// <remarks/>
    //[XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Description { get; set; }

    /// <remarks/>
    //[XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string CreatedWithApplication { get; set; } = string.Empty;

    /// <remarks/>
    //[XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public DateTime CreationTimeCode { get; set; } = DateTime.UtcNow;
}