using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_2.Dto;

/// <remarks/>
[Serializable]
public class GeometryPartDto : TransformablePartDto
{
    [XmlElement(ElementName = "GeometryReference", Type = typeof(GeometryReferenceDto))]
    public GeometrySourceDto GeometrySource { get; set; }

    /// <remarks/>
    [XmlArrayItem("Joint")]
    public List<JointPartDto> Joints { get; set; }

    /// <remarks/>
    [XmlArrayItem("LightEmittingObject")]
    public List<LightEmittingPartDto> LightEmittingObjects { get; set; }

    /// <remarks/>
    [XmlArray("SensorObjects")]
    [XmlArrayItem("SensorObject")]
    public List<SensorDto> Sensors { get; set; }

    /// <remarks/>
    [XmlArrayItem("LightEmittingSurface")]
    public List<LightEmittingSurfaceDto> LightEmittingSurfaces { get; set; }

    /// <remarks/>
    [XmlArrayItem("ElectricalConnector")]
    public List<Vector3Dto> ElectricalConnectors { get; set; }

    /// <remarks/>
    [XmlArrayItem("PendulumConnector")]
    public List<Vector3Dto> PendulumConnectors { get; set; }

    /// <remarks/>
    [XmlAttribute("includedInMeasurement")]
    public bool IncludedInMeasurement { get; set; } = true;

    // ReSharper disable UnusedMember.Global
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingObjects() => LightEmittingObjects != null && LightEmittingObjects.Count > 0;
    [ExcludeFromCodeCoverage] public bool ShouldSerializeSensors() => Sensors != null && Sensors.Count > 0;
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingFaceAssignments() => LightEmittingSurfaces != null && LightEmittingSurfaces.Count > 0;
    [ExcludeFromCodeCoverage] public bool ShouldSerializeJoints() => Joints != null && Joints.Count > 0;
    [ExcludeFromCodeCoverage] public bool ShouldSerializeElectricalConnectors() => ElectricalConnectors != null && ElectricalConnectors.Count > 0;
    [ExcludeFromCodeCoverage] public bool ShouldSerializePendulumConnectors() => PendulumConnectors != null && PendulumConnectors.Count > 0;
    [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingSurfaces() => LightEmittingSurfaces != null && LightEmittingSurfaces.Count > 0;
    [ExcludeFromCodeCoverage] public bool ShouldSerializeIncludedInMeasurement() => !IncludedInMeasurement;
    // ReSharper restore UnusedMember.Global
}