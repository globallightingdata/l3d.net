using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_11_0.Dto;

/// <remarks/>
[Serializable]
public class GeometryPartDto : TransformablePartDto
{
    [XmlElement(ElementName = "GeometryReference")]
    public GeometryReferenceDto GeometryReference { get; set; } = new();

    /// <remarks/>
    [XmlArrayItem("Joint")]
    public JointPartDto[]? Joints { get; set; }

    /// <remarks/>
    [XmlArrayItem("LightEmittingObject")]
    public LightEmittingPartDto[]? LightEmittingObjects { get; set; }

    /// <remarks/>
    [XmlArray("SensorObjects")]
    [XmlArrayItem("SensorObject")]
    public SensorPartDto[]? Sensors { get; set; }

    /// <remarks/>
    [XmlArrayItem("LightEmittingSurface")]
    public LightEmittingSurfacePartDto[]? LightEmittingSurfaces { get; set; }

    /// <remarks/>
    [XmlArrayItem("ElectricalConnector")]
    public Vector3Dto[]? ElectricalConnectors { get; set; }

    /// <remarks/>
    [XmlArrayItem("PendulumConnector")]
    public Vector3Dto[]? PendulumConnectors { get; set; }

    /// <remarks/>
    [XmlAttribute("includedInMeasurement")]
    public bool IncludedInMeasurement { get; set; } = true;

    // ReSharper disable UnusedMember.Global
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingObjects() => LightEmittingObjects is { Length: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeSensors() => Sensors is { Length: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingFaceAssignments() => LightEmittingSurfaces is { Length: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeJoints() => Joints is { Length: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeElectricalConnectors() => ElectricalConnectors is { Length: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializePendulumConnectors() => PendulumConnectors is { Length: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingSurfaces() => LightEmittingSurfaces is { Length: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeIncludedInMeasurement() => !IncludedInMeasurement;
    // ReSharper restore UnusedMember.Global
}