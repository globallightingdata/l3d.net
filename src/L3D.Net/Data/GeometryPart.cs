using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.Data
{
    internal class GeometryPart : TransformablePart
    {
        public GeometryDefinition GeometryDefinition { get; }

        public GeometryPart(string name, GeometryDefinition geometryDefinition) : base(name)
        {
            GeometryDefinition = geometryDefinition;
        }

        public List<JointPart> Joints { get; } = new List<JointPart>();

        public bool ExcludedFromMeasurement { get; set; }

        public List<Vector3> ElectricalConnectors { get; } = new List<Vector3>();

        public List<Vector3> PendulumConnectors { get; } = new List<Vector3>();
        public List<LightEmittingPart> LightEmittingObjects { get; } = new List<LightEmittingPart>();
        public List<LightEmittingFaceAssignment> LightEmittingFaceAssignments { get; } = new List<LightEmittingFaceAssignment>();
        public List<SensorPart> Sensors { get; } = new List<SensorPart>();
    }
}