using L3D.Net.Data;
using L3D.Net.XML.V0_9_2.Dto;
using System;
using System.Linq;

namespace L3D.Net.Mapper.V0_9_2
{
    public class PartMapper : DtoMapperBase<Part, PartDto>
    {
        public static readonly PartMapper Instance = new();

        protected override Part ConvertData(PartDto element) => element switch
        {
            LightEmittingSurfaceDto lightEmittingSurface => new LightEmittingSurfacePart
            {
                Name = lightEmittingSurface.Name,
                FaceAssignments = lightEmittingSurface.FaceAssignments.Select(x => FaceAssignmentMapper.Instance.Convert(x)).ToList(),
                LightEmittingPartIntensityMapping = lightEmittingSurface.LightEmittingPartIntensityMapping.ToDictionary(x => x.LightEmittingPartName, x => x.Intensity)
            },
            JointPartDto jointPart => new JointPart
            {
                Name = jointPart.Name,
                DefaultRotation = Vector3DMapper.Instance.Convert(jointPart.DefaultRotation),
                Position = Vector3DMapper.Instance.Convert(jointPart.Position),
                Rotation = Vector3DMapper.Instance.Convert(jointPart.Rotation),
                XAxis = AxisRotationMapper.Instance.Convert(jointPart.XAxis),
                YAxis = AxisRotationMapper.Instance.Convert(jointPart.YAxis),
                ZAxis = AxisRotationMapper.Instance.Convert(jointPart.ZAxis),
                Geometries = jointPart.Geometries.Select(ConvertData).Cast<GeometryPart>().ToList()
            },
            LightEmittingPartDto lightEmittingPartDto => new LightEmittingPart
            {
                Name = lightEmittingPartDto.Name,
                Position = Vector3DMapper.Instance.Convert(lightEmittingPartDto.Position),
                Rotation = Vector3DMapper.Instance.Convert(lightEmittingPartDto.Rotation),
                LuminousHeights = LuminousHeightsMapper.Instance.Convert(lightEmittingPartDto.LuminousHeights),
                Shape = ShapeMapper.Instance.Convert(lightEmittingPartDto.Shape)
            },
            GeometryPartDto geometryPart => new GeometryPart
            {
                Name = geometryPart.Name,
                ElectricalConnectors = geometryPart.ElectricalConnectors.Select(x => Vector3DMapper.Instance.Convert(x)).ToList(),
                GeometrySource = GeometrySourceMapper.Instance.Convert(geometryPart.GeometrySource),
                Position = Vector3DMapper.Instance.Convert(geometryPart.Position),
                Rotation = Vector3DMapper.Instance.Convert(geometryPart.Rotation),
                IncludedInMeasurement = geometryPart.IncludedInMeasurement,
                Joints = geometryPart.Joints.Select(ConvertData).Cast<JointPart>().ToList(),
                LightEmittingObjects = geometryPart.LightEmittingObjects.Select(ConvertData).Cast<LightEmittingPart>().ToList(),
                LightEmittingSurfaces = geometryPart.LightEmittingSurfaces.Select(ConvertData).Cast<LightEmittingSurfacePart>().ToList(),
                PendulumConnectors = geometryPart.PendulumConnectors.Select(Vector3DMapper.Instance.Convert).ToList(),
                Sensors = geometryPart.Sensors.Select(ConvertData).Cast<SensorPart>().ToList()
            },
            SensorDto sensorObjectDto => new SensorPart
            {
                Name = sensorObjectDto.Name,
                Position = Vector3DMapper.Instance.Convert(sensorObjectDto.Position),
                Rotation = Vector3DMapper.Instance.Convert(sensorObjectDto.Rotation)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(element))
        };

        protected override PartDto ConvertData(Part element) => element switch
        {
            LightEmittingSurfacePart lightEmittingSurface => new LightEmittingSurfaceDto
            {
                Name = lightEmittingSurface.Name,
                FaceAssignments = lightEmittingSurface.FaceAssignments.Select(x => FaceAssignmentMapper.Instance.Convert(x)).ToList(),
                LightEmittingPartIntensityMapping = lightEmittingSurface.LightEmittingPartIntensityMapping.Select(x => new LightEmittingObjectReferenceDto
                {
                    LightEmittingPartName = x.Key,
                    Intensity = x.Value
                }).ToList()
            },
            JointPart jointPart => new JointPartDto
            {
                Name = jointPart.Name,
                DefaultRotation = jointPart.DefaultRotation == null ? null : Vector3DMapper.Instance.Convert(jointPart.DefaultRotation.Value),
                Position = Vector3DMapper.Instance.Convert(jointPart.Position),
                Rotation = Vector3DMapper.Instance.Convert(jointPart.Rotation),
                XAxis = AxisRotationMapper.Instance.Convert(jointPart.XAxis),
                YAxis = AxisRotationMapper.Instance.Convert(jointPart.YAxis),
                ZAxis = AxisRotationMapper.Instance.Convert(jointPart.ZAxis),
                Geometries = jointPart.Geometries.Select(ConvertData).Cast<GeometryPartDto>().ToList(),
            },
            LightEmittingPart lightEmittingPartDto => new LightEmittingPartDto
            {
                Name = lightEmittingPartDto.Name,
                Position = Vector3DMapper.Instance.Convert(lightEmittingPartDto.Position),
                Rotation = Vector3DMapper.Instance.Convert(lightEmittingPartDto.Rotation),
                LuminousHeights = LuminousHeightsMapper.Instance.Convert(lightEmittingPartDto.LuminousHeights),
                Shape = ShapeMapper.Instance.Convert(lightEmittingPartDto.Shape)
            },
            GeometryPart geometryPart => new GeometryPartDto
            {
                Name = geometryPart.Name,
                ElectricalConnectors = geometryPart.ElectricalConnectors.Select(x => Vector3DMapper.Instance.Convert(x)).ToList(),
                GeometrySource = GeometrySourceMapper.Instance.Convert(geometryPart.GeometrySource),
                Position = Vector3DMapper.Instance.Convert(geometryPart.Position),
                Rotation = Vector3DMapper.Instance.Convert(geometryPart.Rotation),
                IncludedInMeasurement = geometryPart.IncludedInMeasurement,
                Joints = geometryPart.Joints.Select(ConvertData).Cast<JointPartDto>().ToList(),
                LightEmittingObjects = geometryPart.LightEmittingObjects.Select(ConvertData).Cast<LightEmittingPartDto>().ToList(),
                LightEmittingSurfaces = geometryPart.LightEmittingSurfaces.Select(ConvertData).Cast<LightEmittingSurfaceDto>().ToList(),
                PendulumConnectors = geometryPart.PendulumConnectors.Select(Vector3DMapper.Instance.Convert).ToList(),
                Sensors = geometryPart.Sensors.Select(ConvertData).Cast<SensorDto>().ToList()
            },
            SensorPart sensorObjectDto => new SensorDto
            {
                Name = sensorObjectDto.Name,
                Position = Vector3DMapper.Instance.Convert(sensorObjectDto.Position),
                Rotation = Vector3DMapper.Instance.Convert(sensorObjectDto.Rotation)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(element))
        };
    }
}
