using System;
using System.Collections.Generic;
using System.Linq;
using L3D.Net.API.Dto;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using CircleDto = L3D.Net.API.Dto.CircleDto;
using GeometryDefinitionDto = L3D.Net.API.Dto.GeometryDefinitionDto;
using HeaderDto = L3D.Net.API.Dto.HeaderDto;
using LuminaireDto = L3D.Net.API.Dto.LuminaireDto;
using LuminousHeightsDto = L3D.Net.API.Dto.LuminousHeightsDto;
using RectangleDto = L3D.Net.API.Dto.RectangleDto;
using ShapeDto = L3D.Net.API.Dto.ShapeDto;

namespace L3D.Net.API;

internal class ApiDtoConverter : IApiDtoConverter
{
    private readonly IFileHandler _fileHandler;

    public ApiDtoConverter(IFileHandler fileHandler)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
    }

    public LuminaireDto Convert(Luminaire luminaire, string directory)
    {
        if (luminaire == null) return null;
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(directory));

        var metaData = Convert(luminaire.Header);
        var geometryDefinitions = luminaire.GeometryDefinitions
            .Select(geomDef => Convert(geomDef, directory))
            .ToList();
        var parts = luminaire.Parts.Select(part => Convert(part, geometryDefinitions)).ToList();

        return new LuminaireDto
        {
            Header = metaData,
            GeometryDefinitions = geometryDefinitions,
            Parts = parts
        };
    }

    internal HeaderDto Convert(Header metaData)
    {
        if (metaData == null)
            return null;

        return new HeaderDto
        {
            CreationTimeCode = metaData.CreationTimeCode,
            Name = metaData.Name,
            Description = metaData.Description,
            CreatedWithApplication = metaData.CreatedWithApplication
        };
    }

    internal GeometryDefinitionDto Convert(GeometryDefinition geometryDefinition,
        string directory)
    {
        if (geometryDefinition == null) return null;
        if (directory == null) throw new ArgumentNullException(nameof(directory));

        double scale;

        switch (geometryDefinition.Units)
        {
            case GeometricUnits.m:
                scale = 1.0;
                break;
            case GeometricUnits.mm:
                scale = 1.0 / 1000.0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return new GeometryDefinitionDto
        {
            Id = geometryDefinition.Id,
            Model = Convert(geometryDefinition.Model, geometryDefinition.Id, (float)scale, directory)
        };
    }

    internal ModelDto Convert(IModel3D model, string geomId, double scale, string directory)
    {
        if (model == null) return null;
        if (string.IsNullOrWhiteSpace(geomId))
            throw new ArgumentException(@$"'{nameof(geomId)}' must not be null or empty!", nameof(geomId));
        if (scale == 0) throw new ArgumentException(@$"'{nameof(scale)}' must not be zero!", nameof(scale));
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(directory));


        return new ModelDto
        {
            Vertices = model.Data.Vertices.Select(vector3 => vector3 * (float)scale).ToArray(),
            Normals = model.Data.Normals.ToArray(),
            TextureCoordinates = model.Data.TextureCoordinates.ToArray(),
            FaceGroups = model.Data.FaceGroups.Select(Convert).ToArray(),
            Materials = model.Data.Materials.Select(material => Convert(material, geomId, directory)).ToArray()
        };
    }

    internal FaceGroupDto Convert(ModelFaceGroup faceGroup)
    {
        if (faceGroup == null)
            return null;

        return new FaceGroupDto
        {
            Name = faceGroup.Name,
            Faces = faceGroup.Faces.Select(Convert).ToList()
        };
    }

    internal FaceDto Convert(ModelFace face)
    {
        if (face == null)
            return null;

        return new FaceDto
        {
            MaterialIndex = face.MaterialIndex,
            Vertices = face.Vertices.Select(Convert).ToList()
        };
    }

    internal FaceVertexDto Convert(ModelFaceVertex vertex)
    {
        if (vertex == null)
            return null;

        return new FaceVertexDto
        {
            VertexIndex = vertex.VertexIndex,
            NormalIndex = vertex.NormalIndex,
            TextureCoordinateIndex = vertex.TextureCoordinateIndex
        };
    }

    internal MaterialDto Convert(ModelMaterial material, string geomId, string directory)
    {
        if (material == null) return null;
        if (string.IsNullOrWhiteSpace(geomId))
            throw new ArgumentException($@"'{nameof(geomId)}' must not be null or empty!", nameof(geomId));
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(directory));

        byte[] textureBytes = null;
        if (!string.IsNullOrWhiteSpace(material.TextureName))
            textureBytes = _fileHandler.GetTextureBytes(directory, geomId, material.TextureName);

        return new MaterialDto
        {
            Name = material.Name,
            Color = material.Color,
            TextureName = material.TextureName,
            TextureBytes = textureBytes
        };
    }

    internal GeometryPartDto Convert(GeometryPart geometry, List<GeometryDefinitionDto> knownGeometryDefinitions)
    {
        if (geometry == null)
            return null;

        if (knownGeometryDefinitions == null)
            throw new ArgumentNullException(nameof(knownGeometryDefinitions));

        return new GeometryPartDto
        {
            Name = geometry.Name,
            Position = geometry.Position,
            Rotation = geometry.Rotation,
            GeometryDefinition =
                knownGeometryDefinitions.FirstOrDefault(dto => dto.Id == geometry.GeometryDefinition.Id) ??
                throw new ArgumentException(
                    $"No geometry definition with id {geometry.GeometryDefinition.Id} available!"),
            Joints = geometry.Joints.Select(joint => Convert(joint, knownGeometryDefinitions)).ToList(),
            LightEmittingObjects = geometry.LightEmittingObjects.Select(Convert).ToList(),
            Sensors = geometry.Sensors.Select(Convert).ToList(),
            LightEmittingSurfaces = geometry.LightEmittingSurfaces.Select(Convert).ToList(),
            ElectricalConnectors = geometry.ElectricalConnectors.ToList(),
            PendulumConnectors = geometry.PendulumConnectors.ToList(),
            IncludedInMeasurement = geometry.IncludedInMeasurement,
        };
    }

    internal JointPartDto Convert(JointPart joint, List<GeometryDefinitionDto> knownGeometryDefinitionDtos)
    {
        if (joint == null)
            return null;

        if (knownGeometryDefinitionDtos == null)
            throw new ArgumentNullException(nameof(knownGeometryDefinitionDtos));

        return new JointPartDto
        {
            Name = joint.Name,
            Position = joint.Position,
            Rotation = joint.Rotation,
            Geometries = joint.Geometries.Select(geomPart => Convert(geomPart, knownGeometryDefinitionDtos))
                .ToList(),
            XAxis = Convert(joint.XAxis),
            YAxis = Convert(joint.YAxis),
            ZAxis = Convert(joint.ZAxis),
            DefaultRotation = joint.DefaultRotation
        };
    }

    internal AxisRotationDto Convert(AxisRotation axis)
    {
        if (axis == null)
            return null;

        return new AxisRotationDto
        {
            Min = axis.Min,
            Max = axis.Max,
            Step = axis.Step
        };
    }

    internal LightEmittingPartDto Convert(LightEmittingPart lightEmittingPart)
    {
        if (lightEmittingPart == null)
            return null;

        return new LightEmittingPartDto
        {
            Name = lightEmittingPart.Name,
            Position = lightEmittingPart.Position,
            Rotation = lightEmittingPart.Rotation,
            Shape = Convert(lightEmittingPart.Shape),
            LuminousHeights = Convert(lightEmittingPart.LuminousHeights)
        };
    }

    internal LuminousHeightsDto Convert(LuminousHeights luminousHeights)
    {
        if (luminousHeights == null)
            return null;

        return new LuminousHeightsDto
        {
            C0 = luminousHeights.C0,
            C90 = luminousHeights.C90,
            C180 = luminousHeights.C180,
            C270 = luminousHeights.C270
        };
    }

    internal ShapeDto Convert(Shape shape)
    {
        if (shape == null)
            return null;

        if (shape is Circle circle)
            return new CircleDto
            {
                Diameter = circle.Diameter
            };

        if (shape is Rectangle rectangle)
            return new RectangleDto
            {
                SizeX = rectangle.SizeX,
                SizeY = rectangle.SizeY
            };

        throw new ArgumentOutOfRangeException(nameof(shape));
    }

    internal SensorPartDto Convert(SensorPart sensor)
    {
        if (sensor == null)
            return null;

        return new SensorPartDto
        {
            Name = sensor.Name,
            Position = sensor.Position,
            Rotation = sensor.Rotation
        };
    }

    internal LightEmittingSurfacePartDto Convert(LightEmittingSurfacePart les)
    {
        if (les == null) return null;

        return new LightEmittingSurfacePartDto
        {
            Name = les.Name,
            LightEmittingObjects = new Dictionary<string, double>(
                les.LightEmittingPartIntensityMapping.ToDictionary(pair => pair.Key, pair => pair.Value)),
            FaceAssignments = les.FaceAssignments.Select<FaceAssignment, BaseAssignmentDto>(assignment =>
            {
                switch (assignment)
                {
                    case SingleFaceAssignment singleAssignment:
                        return new SingleFaceAssignmentDto
                        {
                            GroupIndex = singleAssignment.GroupIndex,
                            FaceIndex = singleAssignment.FaceIndex
                        };
                    case FaceRangeAssignment rangeAssignment:
                        return new RangeFaceAssignmentDto
                        {
                            GroupIndex = rangeAssignment.GroupIndex,
                            FaceIndexBegin = rangeAssignment.FaceIndexBegin,
                            FaceIndexEnd = rangeAssignment.FaceIndexEnd
                        };
                    default: throw new Exception();
                }
            }).ToList()
        };
    }

    internal BaseAssignmentDto Convert(FaceAssignment assignment)
    {
        if (assignment == null)
            return null;

        if (assignment is SingleFaceAssignment single)
            return new SingleFaceAssignmentDto
            {
                GroupIndex = assignment.GroupIndex,
                FaceIndex = single.FaceIndex
            };

        if (assignment is FaceRangeAssignment range)
            return new RangeFaceAssignmentDto
            {
                GroupIndex = assignment.GroupIndex,
                FaceIndexBegin = range.FaceIndexBegin,
                FaceIndexEnd = range.FaceIndexEnd
            };

        throw new ArgumentOutOfRangeException(nameof(assignment));
    }
}