using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.API.Dto
{
    public class GeometryPartDto : PartDto
    {
        public GeometryDefinitionDto GeometryDefinition { get; set; }
        public List<JointPartDto> Joints { get; set; }
        public List<LightEmittingPartDto> LightEmittingObjects { get; set; }
        public List<SensorPartDto> Sensors { get; set; }
        public List<Vector3> ElectricalConnectors { get; set; }
        public List<Vector3> PendulumConnectors { get; set; }
        public bool ExcludedFromMeasurement { get; set; }
        public List<LightEmittingSurfacePartDto> LightEmittingSurfaces { get; set; }
    }
}
