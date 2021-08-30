using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    /// <remarks/>
    [Serializable]
    public class GeometryNodeDto : TransformableNodeDto
    {
        [XmlElement(ElementName = "GeometryReference", Type = typeof(GeometryReferenceDto))]
        public GeometrySourceDto GeometrySource { get; set; }

        /// <remarks/>
        [XmlArrayItem("Joint")]
        public List<JointNodeDto> Joints { get; set; }

        /// <remarks/>
        [XmlArrayItem("LightEmittingObject")]
        public List<LightEmittingNodeDto> LightEmittingObjects { get; set; }

        /// <remarks/>
        [XmlArrayItem("SensorObject")]
        public List<SensorObjectDto> SensorObjects { get; set; }

        /// <remarks/>
        [XmlArrayItem("Assignment", Type = typeof(AssignmentDto))]
        [XmlArrayItem("RangeAssignment", Type = typeof(RangeAssignmentDto))]
        public List<AssignmentBaseDto> LightEmittingFaceAssignments { get; set; }

        /// <remarks/>
        [XmlArrayItem("ElectricalConnector")]
        public List<Vector3Dto> ElectricalConnectors { get; set; }

        /// <remarks/>
        [XmlArrayItem("PendulumConnector")]
        public List<Vector3Dto> PendulumConnectors { get; set; }

        /// <remarks/>
        [XmlAttribute("excludedFromMeasurement")]
        public bool ExcludedFromMeasurement { get; set; }

        // ReSharper disable UnusedMember.Global
        [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingObjects() => LightEmittingObjects != null && LightEmittingObjects.Count > 0;
        [ExcludeFromCodeCoverage] public bool ShouldSerializeSensorObjects() => SensorObjects != null && SensorObjects.Count > 0;
        [ExcludeFromCodeCoverage] public bool ShouldSerializeLightEmittingFaceAssignments() => LightEmittingFaceAssignments != null && LightEmittingFaceAssignments.Count > 0;
        [ExcludeFromCodeCoverage] public bool ShouldSerializeJoints() => Joints != null && Joints.Count > 0;
        [ExcludeFromCodeCoverage] public bool ShouldSerializeElectricalConnectors() => ElectricalConnectors != null && ElectricalConnectors.Count > 0;
        [ExcludeFromCodeCoverage] public bool ShouldSerializePendulumConnectors() => PendulumConnectors != null && PendulumConnectors.Count > 0;
        [ExcludeFromCodeCoverage] public bool ShouldSerializeExcludedFromMeasurement() => ExcludedFromMeasurement;
        // ReSharper restore UnusedMember.Global
    }
}