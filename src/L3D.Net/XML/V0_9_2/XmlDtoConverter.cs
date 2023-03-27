using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_9_2.Dto;
using System;
using System.Linq;
using System.Numerics;
using CircleDto = L3D.Net.XML.V0_9_2.Dto.CircleDto;
using GeometryDefinitionDto = L3D.Net.XML.V0_9_2.Dto.GeometryDefinitionDto;
using HeaderDto = L3D.Net.XML.V0_9_2.Dto.HeaderDto;
using LuminaireDto = L3D.Net.XML.V0_9_2.Dto.LuminaireDto;
using LuminousHeightsDto = L3D.Net.XML.V0_9_2.Dto.LuminousHeightsDto;
using RectangleDto = L3D.Net.XML.V0_9_2.Dto.RectangleDto;
using ShapeDto = L3D.Net.XML.V0_9_2.Dto.ShapeDto;

namespace L3D.Net.XML.V0_9_2;

public class XmlDtoConverter : IXmlDtoConverter
{
    public LuminaireDto Convert(Luminaire luminaire)
    {
        if (luminaire == null)
            return null;

        return new LuminaireDto
        {
            Header = Convert(luminaire.Header),
            GeometryDefinitions = luminaire.GeometryDefinitions.Select(Convert).ToList(),
            Parts = luminaire.Parts.Select(Convert).ToList()
        };
    }

    public HeaderDto Convert(Header metaData)
    {
        if (metaData == null)
            return null;

        return new HeaderDto
        {
            CreatedWithApplication = metaData.CreatedWithApplication,
            CreationTimeCode = metaData.CreationTimeCode,
            Name = metaData.Name,
            Description = metaData.Description
        };
    }

    public GeometricUnitsDto Convert(GeometricUnits units)
    {
        return units switch
        {
            GeometricUnits.m => GeometricUnitsDto.m,
            GeometricUnits.mm => GeometricUnitsDto.mm,
            _ => throw new ArgumentOutOfRangeException(nameof(units), units, null)
        };
    }

    public GeometryDefinitionDto Convert(GeometrySource geometrySource)
    {
        if (geometrySource == null)
            return null;

        return new GeometryFileDefinitionDto
        {
            Id = geometrySource.GeometryId,
            FileName = geometrySource.FileName,
            Units = Convert(geometrySource.Units)
        };
    }

    public GeometryPartDto Convert(GeometryPart geometry)
    {
        if (geometry == null)
            return null;

        return new GeometryPartDto
        {
            Name = geometry.Name,
            Position = Convert(geometry.Position),
            Rotation = Convert(geometry.Rotation),
            GeometrySource = new GeometryReferenceDto { GeometryId = geometry.GeometrySource.GeometryId },
            Joints = geometry.Joints.Select(Convert).ToList(),
            LightEmittingObjects = geometry.LightEmittingObjects.Select(Convert).ToList(),
            Sensors = geometry.Sensors.Select(Convert).ToList(),
            LightEmittingSurfaces = geometry.LightEmittingSurfaces.Select(Convert).ToList(),
            ElectricalConnectors = geometry.ElectricalConnectors.Select(Convert).ToList(),
            PendulumConnectors = geometry.PendulumConnectors.Select(Convert).ToList(),
            IncludedInMeasurement = geometry.IncludedInMeasurement
        };
    }

    public LightEmittingPartDto Convert(LightEmittingPart lightEmittingPart)
    {
        if (lightEmittingPart == null)
            return null;

        return new LightEmittingPartDto
        {
            Name = lightEmittingPart.Name,
            Position = Convert(lightEmittingPart.Position),
            Rotation = Convert(lightEmittingPart.Rotation),
            Shape = Convert(lightEmittingPart.Shape),
            LuminousHeights = Convert(lightEmittingPart.LuminousHeights)
        };
    }

    public LuminousHeightsDto Convert(LuminousHeights luminousHeights)
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

    public Vector3Dto Convert(Vector3 vector)
    {
        return new Vector3Dto
        {
            X = vector.X,
            Y = vector.Y,
            Z = vector.Z
        };
    }

    public JointPartDto Convert(JointPart joint)
    {
        if (joint == null)
            return null;

        return new JointPartDto
        {
            Name = joint.Name,
            Position = Convert(joint.Position),
            Rotation = Convert(joint.Rotation),
            XAxis = Convert(joint.XAxis),
            YAxis = Convert(joint.YAxis),
            ZAxis = Convert(joint.ZAxis),
            DefaultRotation = Convert(joint.DefaultRotation),
            Geometries = joint.Geometries.Select(Convert).ToList()
        };
    }

    public Vector3Dto Convert(Vector3? vector)
    {
        if (vector == null)
            return null;

        return new Vector3Dto
        {
            X = vector.Value.X,
            Y = vector.Value.Y,
            Z = vector.Value.Z
        };
    }

    public AxisRotationDto Convert(AxisRotation axisRotation)
    {
        if (axisRotation == null)
            return null;

        return new AxisRotationDto
        {
            Min = axisRotation.Min,
            Max = axisRotation.Max,
            Step = axisRotation.Step
        };
    }

    public SensorDto Convert(SensorPart sensor)
    {
        if (sensor == null)
            return null;

        return new SensorDto
        {
            Name = sensor.Name,
            Position = Convert(sensor.Position),
            Rotation = Convert(sensor.Rotation)
        };
    }

    public ShapeDto Convert(Shape shape)
    {
        if (shape == null)
            return null;

        if (shape is Circle circle)
        {
            return new CircleDto
            {
                Diameter = circle.Diameter
            };
        }

        if (shape is Rectangle rectangle)
        {
            return new RectangleDto
            {
                SizeX = rectangle.SizeX,
                SizeY = rectangle.SizeY
            };
        }

        throw new ArgumentOutOfRangeException($"Unknown shape type: {shape.GetType().FullName}");
    }

    public FaceAssignmentDto Convert(FaceAssignment assignment)
    {
        return assignment switch
        {
            null => null,
            SingleFaceAssignment single => new SingleFaceAssignmentDto
            {
                GroupIndex = single.GroupIndex,
                FaceIndex = single.FaceIndex
            },
            FaceRangeAssignment range => new FaceRangeAssignmentDto
            {
                GroupIndex = range.GroupIndex,
                FaceIndexBegin = range.FaceIndexBegin,
                FaceIndexEnd = range.FaceIndexEnd
            },
            _ => throw new ArgumentOutOfRangeException($"Unknown assignment type {assignment.GetType().FullName}")
        };
    }

    public LightEmittingSurfaceDto Convert(LightEmittingSurfacePart les)
    {
        if (les == null)
            return null;

        return new LightEmittingSurfaceDto
        {
            Name = les.Name,
            LightEmittingPartIntensityMapping = les.LightEmittingPartIntensityMapping.Select(entry =>
                new LightEmittingObjectReferenceDto
                {
                    LightEmittingPartName = entry.Key,
                    Intensity = entry.Value
                }).ToList(),
            FaceAssignments = les.FaceAssignments.Select(Convert).ToList()
        };
    }
}