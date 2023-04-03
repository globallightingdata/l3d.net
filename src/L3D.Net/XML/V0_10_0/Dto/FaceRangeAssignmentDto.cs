using System;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_10_0.Dto;

/// <remarks/>
[Serializable]
public class FaceRangeAssignmentDto : FaceAssignmentDto
{
    /// <remarks/>
    [XmlAttribute("faceIndexBegin")]
    public int FaceIndexBegin { get; set; }

    /// <remarks/>
    [XmlAttribute("faceIndexEnd")]
    public int FaceIndexEnd { get; set; }
}