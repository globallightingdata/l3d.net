using System;
using System.IO;
using System.Linq;
using System.Numerics;
using L3D.Net.API.Dto;
using L3D.Net.BuilderOptions;
using L3D.Net.Data;
using L3D.Net.XML.V0_9_2.Dto;
using CircleDto = L3D.Net.XML.V0_9_2.Dto.CircleDto;
using LuminaireDto = L3D.Net.XML.V0_9_2.Dto.LuminaireDto;
using RectangleDto = L3D.Net.XML.V0_9_2.Dto.RectangleDto;

namespace L3D.Net.XML.V0_9_2
{
    class LuminaireFromDtoConstructor : ILuminaireFromDtoConstructor
    {
        public Luminaire BuildLuminaireFromDto(LuminaireBuilder builder, LuminaireDto luminaireDto, string dataDirectory)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (luminaireDto == null) throw new ArgumentNullException(nameof(luminaireDto));
            if (string.IsNullOrWhiteSpace(dataDirectory))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(dataDirectory));

            if (luminaireDto.Header == null)
                throw new ArgumentException(@"Luminaire must contain MetaData", nameof(luminaireDto));

            if (luminaireDto.GeometryDefinitions == null)
                throw new ArgumentException(@"Luminaire must contain a GeometryDefinition list", nameof(luminaireDto));

            if (luminaireDto.Parts == null)
                throw new ArgumentException(@"Luminaire must contain a Structure list", nameof(luminaireDto));

            builder
                .WithCreatedTime(luminaireDto.Header.CreationTimeCode)
                .WithTool(luminaireDto.Header.CreatedWithApplication)
                .WithModelName(luminaireDto.Header.Name)
                .WithDescription(luminaireDto.Header.Description);

            foreach (var geometryNodeDto in luminaireDto.Parts)
            {
                var (modelPath, units) = GetModelFilePathAndUnits(luminaireDto, geometryNodeDto, dataDirectory);
                builder.AddGeometry(geometryNodeDto.PartName, modelPath, units, options => SetupGeometryPart(options, luminaireDto, geometryNodeDto, dataDirectory));
            }

            return builder.Luminaire;
        }

        private GeometryOptions SetupGeometryPart(GeometryOptions options, LuminaireDto luminaireDto, GeometryNodeDto geometryNodeDto, string directory)
        {
            options.WithIncludedInMeasurement(geometryNodeDto.IncludedInMeasurement);

            foreach (var jointDto in geometryNodeDto.Joints)
            {
                options.AddJoint(jointDto.PartName, jointOptions => SetupJointPart(jointOptions, luminaireDto, jointDto, directory));
            }

            foreach (var lightEmittingNodeDto in geometryNodeDto.LightEmittingObjects)
            {
                if (lightEmittingNodeDto.Shape is CircleDto circleDto)
                    options.AddCircularLightEmittingObject(lightEmittingNodeDto.PartName, circleDto.Diameter, leoOptions => SetupLightEmittingPart(leoOptions, lightEmittingNodeDto));
                else if (lightEmittingNodeDto.Shape is RectangleDto rectangleDto)
                    options.AddRectangularLightEmittingObject(lightEmittingNodeDto.PartName, rectangleDto.SizeX, rectangleDto.SizeY, leoOptions => SetupLightEmittingPart(leoOptions, lightEmittingNodeDto));
                else
                    throw new Exception($"Invalid Shape type in LightEmittingNodeDto: {lightEmittingNodeDto.Shape?.GetType().FullName}");
            }

            foreach (var sensorObject in geometryNodeDto.SensorObjects)
            {
                options.AddSensorObject(sensorObject.PartName, sensorOptions => SetupSensorPart(sensorOptions, sensorObject));
            }

            foreach (var les in geometryNodeDto.LightEmittingSurfaces)
            {
                options.WithLightEmittingSurface(les.PartName, lesOptions =>
                {
                    if (les.LightEmittingObjectReference == null) throw new Exception("'LightEmittingObjectReference' must not be null!");
                    if (les.FaceAssignments == null) throw new Exception("'LightEmittingObjectReference' must not be null!");
                    
                    les.LightEmittingObjectReference.ForEach(leoRef => lesOptions.WithLightEmittingPart(leoRef.LightEmittingPartName, leoRef.Intensity));
                    les.FaceAssignments.ForEach(assignment =>
                    {
                        switch (assignment)
                        {
                            case FaceAssignmentDto singleAssignement:
                                lesOptions.WithSurface(singleAssignement.FaceIndex, singleAssignement.GroupIndex);
                                break;
                            case FaceRangeAssignmentDto rangeAssignment:
                                lesOptions.WithSurfaceRange(rangeAssignment.FaceIndexBegin, rangeAssignment.FaceIndexEnd, rangeAssignment.GroupIndex);
                                break;
                            default:
                                throw new Exception("Invalid AssignmentDto in GeometryNodeDto: " + les?.GetType().FullName);
                        }
                    });

                    return lesOptions;
                });
                
            }

            foreach (var electricalConnector in geometryNodeDto.ElectricalConnectors)
            {
                options.WithElectricalConnector(Convert(electricalConnector));
            }

            foreach (var pendulumConnector in geometryNodeDto.PendulumConnectors)
            {
                options.WithPendulumConnector(Convert(pendulumConnector));
            }

            SetupTransformablePart(options, geometryNodeDto);

            return options;
        }


        private void SetupTransformablePart(TransformableOptions options, TransformableNodeDto transformableDto)
        {
            options.WithPosition(Convert(transformableDto.Position));
            options.WithRotation(Convert(transformableDto.Rotation));
        }

        private LightEmittinObjectOptions SetupLightEmittingPart(LightEmittinObjectOptions options, LightEmittingNodeDto lightEmittingNodeDto)
        {
            var heights = lightEmittingNodeDto.LuminousHeights;
            if (heights != null)
                options.WithLuminousHeights(heights.C0, heights.C90, heights.C180, heights.C270);

            SetupTransformablePart(options, lightEmittingNodeDto);
            return options;
        }

        private JointOptions SetupJointPart(JointOptions options, LuminaireDto luminaireDto, JointNodeDto jointDto, string directory)
        {
            var defaultRotation = jointDto.DefaultRotation;
            if (defaultRotation != null)
                options.WithDefaultRotation(Convert(defaultRotation));

            var xAxis = jointDto.XAxis;
            if (xAxis != null)
                options.WithXAxisDegreesOfFreedom(xAxis.Min, xAxis.Max, xAxis.Step);

            var yAxis = jointDto.YAxis;
            if (yAxis != null)
                options.WithYAxisDegreesOfFreedom(yAxis.Min, yAxis.Max, yAxis.Step);

            var zAxis = jointDto.ZAxis;
            if (zAxis != null)
                options.WithZAxisDegreesOfFreedom(zAxis.Min, zAxis.Max, zAxis.Step);

            foreach (var geometryDto in jointDto.Geometries)
            {
                var (modelFilePath, units) = GetModelFilePathAndUnits(luminaireDto, geometryDto, directory);
                options.AddGeometry(geometryDto.PartName, modelFilePath, units, geometryOptions => SetupGeometryPart(geometryOptions, luminaireDto, geometryDto, directory));
            }

            SetupTransformablePart(options, jointDto);
            return options;
        }

        private SensorOptions SetupSensorPart(SensorOptions options, SensorObjectDto sensorObjectDto)
        {
            SetupTransformablePart(options, sensorObjectDto);
            return options;
        }

        private Vector3 Convert(Vector3Dto vector3Dto)
        {
            if (vector3Dto == null)
                return Vector3.Zero;

            return new Vector3(vector3Dto.X, vector3Dto.Y, vector3Dto.Z);
        }

        private GeometricUnits Convert(GeometryNodeUnits units)
        {
            switch (units)
            {
                case GeometryNodeUnits.m:
                    return GeometricUnits.m;
                case GeometryNodeUnits.mm:
                    return GeometricUnits.mm;
                default:
                    throw new ArgumentOutOfRangeException(nameof(units), units, null);
            }
        }

        private (string modelFilePath, GeometricUnits units) GetModelFilePathAndUnits(LuminaireDto luminaireDto, GeometryNodeDto geometryNodeDto, string directory)
        {
            if (geometryNodeDto.GeometrySource == null)
                throw new ArgumentException("GeometryNodeDto must have a GeometrySource!");

            if (!(geometryNodeDto.GeometrySource is GeometryReferenceDto geometryReference))
                throw new Exception("Unknown GeometrySource type in GeometryNodeDto: " + geometryNodeDto.GeometrySource.GetType().FullName);

            var geometryId = geometryReference.GeometryId;

            var geometryDefinition = luminaireDto.GeometryDefinitions.FirstOrDefault(geomDefdto => geomDefdto.Id == geometryId) 
                                     ?? throw new ArgumentException($"LuminaireDto has no geometry definition with the id {geometryId}");

            if (!(geometryDefinition is GeometryFileDefinitionDto geometryFileDefinition))
                throw new Exception("Unknown GeometryDefinitionDto type in LuminaireDto: " + geometryDefinition.GetType().FullName);

            var modelPath = Path.Combine(directory, geometryId, geometryFileDefinition.Filename);

            return (modelPath, Convert(geometryFileDefinition.Units));
        }
    }
}
