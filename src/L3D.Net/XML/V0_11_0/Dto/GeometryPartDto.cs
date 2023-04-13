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
    public List<JointPartDto>? Joints { get; set; }

    /// <remarks/>
    [XmlArrayItem("LightEmittingObject")]
    public List<LightEmittingPartDto>? LightEmittingObjects { get; set; }

    /// <remarks/>
    [XmlArray("SensorObjects")]
    [XmlArrayItem("SensorObject")]
    public List<SensorPartDto>? Sensors { get; set; }

    /// <remarks/>
    [XmlArrayItem("LightEmittingSurface")]
    public List<LightEmittingSurfacePartDto>? LightEmittingSurfaces { get; set; }

    /// <remarks/>
    [XmlArrayItem("ElectricalConnector")]
    public List<Vector3Dto>? ElectricalConnectors { get; set; }

    /// <remarks/>
    [XmlArrayItem("PendulumConnector")]
    public List<Vector3Dto>? PendulumConnectors { get; set; }

    /// <remarks/>
    [XmlAttribute("includedInMeasurement")]
    public bool IncludedInMeasurement { get; set; } = true;

    // ReSharper disable UnusedMember.Global
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingObjects() => LightEmittingObjects is { Count: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeSensors() => Sensors is { Count: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingFaceAssignments() => LightEmittingSurfaces is { Count: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeJoints() => Joints is { Count: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeElectricalConnectors() => ElectricalConnectors is { Count: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializePendulumConnectors() => PendulumConnectors is { Count: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingSurfaces() => LightEmittingSurfaces is { Count: > 0 };
    [ExcludeFromCodeCoverage] public bool ShouldSerializeIncludedInMeasurement() => !IncludedInMeasurement;
    // ReSharper restore UnusedMember.Global
}