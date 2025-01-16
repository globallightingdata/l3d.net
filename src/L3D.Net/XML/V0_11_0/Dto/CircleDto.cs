﻿using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_11_0.Dto;

/// <remarks />
[Serializable]
//[XmlType(AnonymousType = true)]
public class CircleDto : ShapeDto
{
    /// <remarks />
    [XmlAttribute("diameter")]
    public double Diameter { get; set; }
}