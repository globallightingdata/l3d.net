using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_10_0.Dto;

/// <remarks/>
public abstract class FaceAssignmentDto
{
    /// <remarks/>
    [XmlAttribute("groupIndex")]
    public int GroupIndex { get; set; }

    // ReSharper disable UnusedMember.Global

    [ExcludeFromCodeCoverage]
    public bool ShouldSerializeGroupIndex() => GroupIndex != 0;

    // ReSharper restore UnusedMember.Global
};