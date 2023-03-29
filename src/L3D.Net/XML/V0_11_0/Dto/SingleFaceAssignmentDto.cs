using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_11_0.Dto;

/// <remarks/>
[Serializable]
public class SingleFaceAssignmentDto : FaceAssignmentDto
{
    /// <remarks/>
    [XmlAttribute("faceIndex")]
    public int FaceIndex { get; set; }
}