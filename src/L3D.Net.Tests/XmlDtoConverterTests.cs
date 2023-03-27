using FluentAssertions;
using FluentAssertions.Equivalency;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_9_2;
using L3D.Net.XML.V0_9_2.Dto;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CircleDto = L3D.Net.XML.V0_9_2.Dto.CircleDto;
using HeaderDto = L3D.Net.XML.V0_9_2.Dto.HeaderDto;
using LuminaireDto = L3D.Net.XML.V0_9_2.Dto.LuminaireDto;
using LuminousHeightsDto = L3D.Net.XML.V0_9_2.Dto.LuminousHeightsDto;
using RectangleDto = L3D.Net.XML.V0_9_2.Dto.RectangleDto;

// ReSharper disable ExpressionIsAlwaysNull

namespace L3D.Net.Tests;

[TestFixture]
public class XmlDtoConverterTests
{
    private readonly Random _random = new();

    private Vector3 NextVector3()
    {
        return new Vector3((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble());
    }

    private int NextPositiveInt()
    {
        return 1 + _random.Next(10000);
    }

    private static EquivalencyAssertionOptions<T> Config<T>(EquivalencyAssertionOptions<T> options)
    {
        return options
            .IncludingAllRuntimeProperties()
            .AllowingInfiniteRecursion();
    }

    [Test]
    public void Convert_NullableVector3_ShouldReturnNull_WhenVectorIsNull()
    {
        Vector3? vector = null;

        var converted = new XmlDtoConverter().Convert(vector);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_NullableVector3_ShouldReturnExpectedDto()
    {
        var x = (float)_random.NextDouble();
        var y = (float)_random.NextDouble();
        var z = (float)_random.NextDouble();

        var expectedDto = new Vector3Dto
        {
            X = x,
            Y = y,
            Z = z
        };

        Vector3? vector = new Vector3(x, y, z);

        var converted = new XmlDtoConverter().Convert(vector);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_Vector3_ShouldReturnExpectedDto()
    {
        var x = (float)_random.NextDouble();
        var y = (float)_random.NextDouble();
        var z = (float)_random.NextDouble();

        var expectedDto = new Vector3Dto
        {
            X = x,
            Y = y,
            Z = z
        };

        var vector = new Vector3(x, y, z);

        var converted = new XmlDtoConverter().Convert(vector);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_AxisRotation_ShouldReturnNull_WhenVectorIsNull()
    {
        AxisRotation axisRotation = null;

        var converted = new XmlDtoConverter().Convert(axisRotation);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_AxisRotation_ShouldReturnExpectedDto()
    {
        var min = _random.NextDouble();
        var max = min + _random.NextDouble();
        var step = 0.01 + _random.NextDouble();

        var expectedDto = new AxisRotationDto()
        {
            Min = min,
            Max = max,
            Step = step
        };

        var vector = new AxisRotation
        {
            Min = min,
            Max = max,
            Step = step
        };

        var converted = new XmlDtoConverter().Convert(vector);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_Shape_ShouldReturnNull_WhenShapeIsNull()
    {
        Shape shape = null;

        var converted = new XmlDtoConverter().Convert(shape);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_Shape_ShouldReturnCircleDto_WhenShapeIsCircle()
    {
        var diameter = 0.001 + _random.NextDouble();
        Shape shape = new Circle
        {
            Diameter = diameter
        };
        var expectedDto = new CircleDto
        {
            Diameter = diameter
        };

        var converted = new XmlDtoConverter().Convert(shape);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_Shape_ShouldReturnRectangleDto_WhenShapeIsRectangle()
    {
        var sizeX = 0.001 + _random.NextDouble();
        var sizeY = 0.001 + _random.NextDouble();
        Shape shape = new Rectangle
        {
            SizeX = sizeX,
            SizeY = sizeY
        };
        var expectedDto = new RectangleDto
        {
            SizeX = sizeX,
            SizeY = sizeY
        };

        var converted = new XmlDtoConverter().Convert(shape);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    class UnknownShape : Shape
    {

    }

    [Test]
    public void Convert_Shape_ShouldThrowArgumentOutOfRangeException_WhenShapeIsUnknownType()
    {
        Action action = () => new XmlDtoConverter().Convert(new UnknownShape());

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Convert_LuminousHeights_ShouldReturnNull_WhenLuminousHeightsArgIsNull()
    {
        LuminousHeights luminousHeights = null;

        var converted = new XmlDtoConverter().Convert(luminousHeights);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_LuminousHeights_ShouldReturnCorrectDto()
    {
        var c0 = _random.NextDouble();
        var c90 = _random.NextDouble();
        var c180 = _random.NextDouble();
        var c270 = _random.NextDouble();

        var expectedDto = new LuminousHeightsDto
        {
            C0 = c0,
            C90 = c90,
            C180 = c180,
            C270 = c270
        };

        var luminousHeights = new LuminousHeights
        {
            C0 = c0,
            C90 = c90,
            C180 = c180,
            C270 = c270
        };

        var converted = new XmlDtoConverter().Convert(luminousHeights);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_SensorPart_ShouldReturnNull_WhenSensorIsNull()
    {
        SensorPart sensorPart = null;

        var converted = new XmlDtoConverter().Convert(sensorPart);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_SensorPart_ShouldReturnCorrectDto()
    {
        var partName = Guid.NewGuid().ToString();
        var position = NextVector3();
        var rotation = NextVector3();

        var expectedDto = new SensorDto
        {
            Name = partName,
            Position = new Vector3Dto
            {
                X = position.X,
                Y = position.Y,
                Z = position.Z
            },
            Rotation = new Vector3Dto
            {
                X = rotation.X,
                Y = rotation.Y,
                Z = rotation.Z
            }
        };

        var converted = new XmlDtoConverter().Convert(new SensorPart
        {
            Name = partName,
            Position = position,
            Rotation = rotation
        });

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_LightEmittingFaceAssignment_ShouldReturnNull_WhenArgumentIsNull()
    {
        FaceAssignment assignment = null;

        var converted = new XmlDtoConverter().Convert(assignment);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_LightEmittingFaceAssignment_ShouldReturnCorrectDto_WhenArgumentIsSingleLightEmittingFaceAssignment()
    {
        var groupIndex = NextPositiveInt();
        var faceIndex = NextPositiveInt();
        var assignment = new SingleFaceAssignment
        {
            GroupIndex = groupIndex,
            FaceIndex = faceIndex
        };
        var expectedDto = new SingleFaceAssignmentDto
        {
            GroupIndex = groupIndex,
            FaceIndex = faceIndex
        };

        var converted = new XmlDtoConverter().Convert(assignment);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_LightEmittingFaceAssignment_ShouldReturnCorrectDto_WhenArgumentIsRangeLightEmittingFaceAssignment()
    {
        var groupIndex = NextPositiveInt();
        var faceIndexBegin = NextPositiveInt();
        var faceIndexEnd = faceIndexBegin + NextPositiveInt();
        var assignment = new FaceRangeAssignment
        {
            GroupIndex = groupIndex,
            FaceIndexBegin = faceIndexBegin,
            FaceIndexEnd = faceIndexEnd
        };
        var expectedDto = new FaceRangeAssignmentDto
        {
            GroupIndex = groupIndex,
            FaceIndexBegin = faceIndexBegin,
            FaceIndexEnd = faceIndexEnd
        };

        var converted = new XmlDtoConverter().Convert(assignment);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    class UnknownAssignement : FaceAssignment
    {
        public UnknownAssignement(int groupIndex)
        {
            GroupIndex = groupIndex;
        }
    }

    [Test]
    public void Convert_LightEmittingFaceAssignment_ShouldThrowArgumentOutOfRangeException_WhenAssignmentTypeIsUnknown()
    {
        var groupIndex = NextPositiveInt();

        var assignment = new UnknownAssignement(groupIndex);

        Action action = () => new XmlDtoConverter().Convert(assignment);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Convert_JointPart_ShouldReturnNull_WhenArgumentIsNull()
    {
        JointPart jointPart = null;

        var converted = new XmlDtoConverter().Convert(jointPart);

        converted.Should().BeNull();
    }

    AxisRotation NextAxisRotation()
    {
        var min = _random.NextDouble();
        var max = min + _random.NextDouble();
        var step = 0.01 + _random.NextDouble();

        return new AxisRotation
        {
            Min = min,
            Max = max,
            Step = step
        };
    }

    [Test]
    public void Convert_JointPart_ShouldReturnCorrectFullDto()
    {
        var jointPartName = Guid.NewGuid().ToString();
        var geomPartName = Guid.NewGuid().ToString();
        var geomDefId = Guid.NewGuid().ToString();
        var position = NextVector3();
        var rotation = NextVector3();
        var defaultRotation = NextVector3();
        var xAxis = NextAxisRotation();
        var yAxis = NextAxisRotation();
        var zAxis = NextAxisRotation();

        var jointPart = new JointPart
        {
            Name = jointPartName,
            Position = position,
            Rotation = rotation,
            Geometries =
            {
                new GeometryPart
                {
                    Name = geomPartName,
                    GeometrySource = new GeometrySource
                    {
                        GeometryId = geomDefId,
                        Model = Substitute.For<IModel3D>(),
                        Units = GeometricUnits.m
                    }
                }
            },
            DefaultRotation = defaultRotation,
            XAxis = xAxis,
            YAxis = yAxis,
            ZAxis = zAxis
        };

        var converter = new XmlDtoConverter();

        var expectedDto = new JointPartDto
        {
            Name = jointPartName,
            Position = new Vector3Dto
            {
                X = position.X,
                Y = position.Y,
                Z = position.Z
            },
            Rotation = new Vector3Dto
            {
                X = rotation.X,
                Y = rotation.Y,
                Z = rotation.Z
            },
            Geometries = jointPart.Geometries.Select(part => converter.Convert(part)).ToList(),
            DefaultRotation = new Vector3Dto
            {
                X = defaultRotation.X,
                Y = defaultRotation.Y,
                Z = defaultRotation.Z
            },
            XAxis = new AxisRotationDto
            {
                Min = xAxis.Min,
                Max = xAxis.Max,
                Step = xAxis.Step
            },
            YAxis = new AxisRotationDto
            {
                Min = yAxis.Min,
                Max = yAxis.Max,
                Step = yAxis.Step
            },
            ZAxis = new AxisRotationDto
            {
                Min = zAxis.Min,
                Max = zAxis.Max,
                Step = zAxis.Step
            }
        };

        var converted = converter.Convert(jointPart);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_JointPart_ShouldReturnCorrectSimpleDto()
    {
        var jointPartName = Guid.NewGuid().ToString();
        var position = NextVector3();
        var rotation = NextVector3();

        var jointPart = new JointPart
        {
            Name = jointPartName,
            Position = position,
            Rotation = rotation
        };

        var expectedDto = new JointPartDto
        {
            Name = jointPartName,
            Position = new Vector3Dto
            {
                X = position.X,
                Y = position.Y,
                Z = position.Z
            },
            Rotation = new Vector3Dto
            {
                X = rotation.X,
                Y = rotation.Y,
                Z = rotation.Z
            },
            XAxis = new(),
            YAxis = new(),
            ZAxis = new(),
            Geometries = new List<GeometryPartDto>()
        };

        var converted = new XmlDtoConverter().Convert(jointPart);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_LightEmittingPart_ShouldReturnNull_WhenArgumentIsNull()
    {
        LightEmittingPart lightEmittingPart = null;

        var converted = new XmlDtoConverter().Convert(lightEmittingPart);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_LightEmittingPart_ShouldReturnCorrectFullDto()
    {
        var partName = Guid.NewGuid().ToString();
        var shape = new Circle
        {
            Diameter = 0.0001 + _random.NextDouble()
        };
        var position = NextVector3();
        var rotation = NextVector3();
        var luminousHeights = new LuminousHeights
        {
            C0 = _random.NextDouble(),
            C90 = _random.NextDouble(),
            C180 = _random.NextDouble(),
            C270 = _random.NextDouble()
        };
        var lightEmittingPart = new LightEmittingPart
        {
            Name = partName,
            Shape = shape,
            Position = position,
            Rotation = rotation,
            LuminousHeights = luminousHeights
        };
        var expectedDto = new LightEmittingPartDto
        {
            Name = partName,
            Position = new Vector3Dto
            {
                X = position.X,
                Y = position.Y,
                Z = position.Z
            },
            Rotation = new Vector3Dto
            {
                X = rotation.X,
                Y = rotation.Y,
                Z = rotation.Z
            },
            Shape = new CircleDto
            {
                Diameter = shape.Diameter
            },
            LuminousHeights = new LuminousHeightsDto
            {
                C0 = luminousHeights.C0,
                C90 = luminousHeights.C90,
                C180 = luminousHeights.C180,
                C270 = luminousHeights.C270,
            }
        };

        var converted = new XmlDtoConverter().Convert(lightEmittingPart);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_LightEmittingPart_ShouldReturnCorrectSimpleDto()
    {
        var partName = Guid.NewGuid().ToString();
        var shape = new Rectangle
        {
            SizeX = 0.0001 + _random.NextDouble(),
            SizeY = 0.0001 + _random.NextDouble()
        };
        var lightEmittingPart = new LightEmittingPart
        {
            Name = partName,
            Shape = shape
        };
        var expectedDto = new LightEmittingPartDto
        {
            Name = partName,
            Position = new Vector3Dto
            {
                X = 0,
                Y = 0,
                Z = 0
            },
            Rotation = new Vector3Dto
            {
                X = 0,
                Y = 0,
                Z = 0
            },
            Shape = new RectangleDto
            {
                SizeX = shape.SizeX,
                SizeY = shape.SizeY,
            },
            LuminousHeights = new()
        };

        var converted = new XmlDtoConverter().Convert(lightEmittingPart);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_GeometryPart_ShouldReturnNull_WhenArgumentIsNull()
    {
        GeometryPart geometryPart = null;

        var converted = new XmlDtoConverter().Convert(geometryPart);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_GeometryPart_ShouldReturnCorrectFullDto()
    {
        var partName = Guid.NewGuid().ToString();
        var geomDefId = Guid.NewGuid().ToString();
        var geometryDefinition = new GeometrySource
        {
            GeometryId = geomDefId,
            Model = Substitute.For<IModel3D>(),
            Units = GeometricUnits.m
        };
        var position = NextVector3();
        var rotation = NextVector3();
        var jointPart = new JointPart
        {
            Name = Guid.NewGuid().ToString()
        };
        var leoPart = new LightEmittingPart
        {
            Name = Guid.NewGuid().ToString(),
            Shape = new Circle
            {
                Diameter = 0.0001 + _random.NextDouble()
            }
        };
        var sensorPart = new SensorPart
        {
            Name = Guid.NewGuid().ToString()
        };
        var electricalConnector = NextVector3();
        var pendulumConnector = NextVector3();
        var lightEmittingSurface = new LightEmittingSurfacePart
        {
            Name = Guid.NewGuid().ToString()
        };
        var geometryPart = new GeometryPart
        {
            Name = partName,
            GeometrySource = geometryDefinition,
            Position = position,
            Rotation = rotation,
            IncludedInMeasurement = true,
            Joints = { jointPart },
            LightEmittingObjects = { leoPart },
            Sensors = { sensorPart },
            ElectricalConnectors = { electricalConnector },
            PendulumConnectors = { pendulumConnector },
            LightEmittingSurfaces = { lightEmittingSurface }
        };

        var converter = new XmlDtoConverter();

        var expectedDto = new GeometryPartDto
        {
            Name = partName,
            Position = new Vector3Dto
            {
                X = position.X,
                Y = position.Y,
                Z = position.Z
            },
            Rotation = new Vector3Dto
            {
                X = rotation.X,
                Y = rotation.Y,
                Z = rotation.Z
            },
            IncludedInMeasurement = true,
            GeometrySource = new GeometryReferenceDto { GeometryId = geomDefId },
            Joints = geometryPart.Joints.Select(part => converter.Convert(part)).ToList(),
            LightEmittingObjects = geometryPart.LightEmittingObjects.Select(part => converter.Convert(part)).ToList(),
            Sensors = geometryPart.Sensors.Select(part => converter.Convert(part)).ToList(),
            LightEmittingSurfaces = geometryPart.LightEmittingSurfaces.Select(faceAssignment => converter.Convert(faceAssignment)).ToList(),
            PendulumConnectors = geometryPart.PendulumConnectors.Select(v => converter.Convert(v)).ToList(),
            ElectricalConnectors = geometryPart.ElectricalConnectors.Select(v => converter.Convert(v)).ToList(),
        };

        var converted = converter.Convert(geometryPart);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_GeometryPart_ShouldReturnCorrectSimpleDto()
    {
        var partName = Guid.NewGuid().ToString();
        var geomDefId = Guid.NewGuid().ToString();
        var geometryDefinition = new GeometrySource
        {
            GeometryId = geomDefId,
            Model = Substitute.For<IModel3D>(),
            Units = GeometricUnits.m
        };
        var geometryPart = new GeometryPart
        {
            Name = partName,
            GeometrySource = geometryDefinition
        };

        var converter = new XmlDtoConverter();

        var expectedDto = new GeometryPartDto
        {
            Name = partName,
            Position = new Vector3Dto(),
            Rotation = new Vector3Dto(),
            IncludedInMeasurement = true,
            GeometrySource = new GeometryReferenceDto { GeometryId = geomDefId },
            Joints = new List<JointPartDto>(),
            LightEmittingObjects = new List<LightEmittingPartDto>(),
            Sensors = new List<SensorDto>(),
            LightEmittingSurfaces = new List<LightEmittingSurfaceDto>(),
            PendulumConnectors = new List<Vector3Dto>(),
            ElectricalConnectors = new List<Vector3Dto>(),
        };

        var converted = converter.Convert(geometryPart);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_GeometryDefinition_ShouldReturnNull_WhenArgumentIsNull()
    {
        GeometrySource geometrySource = null;

        var converted = new XmlDtoConverter().Convert(geometrySource);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_GeometryDefintion_ShouldReturnCorrectDto()
    {
        var id = Guid.NewGuid().ToString();
        var modelPath = Guid.NewGuid().ToString();
        var geometryDefinition = new GeometrySource
        {
            GeometryId = id,
            FileName = modelPath,
            Units = GeometricUnits.m
        };
        var expectedDto = new GeometryFileDefinitionDto
        {
            Id = id,
            FileName = modelPath,
            Units = GeometricUnitsDto.m
        };

        var converted = new XmlDtoConverter().Convert(geometryDefinition);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }

    [Test]
    public void Convert_LuminaireMetaData_ShouldReturnNull_WhenArgumentIsNull()
    {
        Header metaData = null;

        var converted = new XmlDtoConverter().Convert(metaData);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_LuminaireMetaData_ShouldReturnCorrectFullDto()
    {
        var metaData = new Header
        {
            CreatedWithApplication = Guid.NewGuid().ToString(),
            Name = Guid.NewGuid().ToString(),
            Description = Guid.NewGuid().ToString(),
            CreationTimeCode = DateTime.Now
        };

        var expectedDto = new HeaderDto
        {
            CreatedWithApplication = metaData.CreatedWithApplication,
            Name = metaData.Name,
            Description = metaData.Description,
            CreationTimeCode = metaData.CreationTimeCode
        };

        var converted = new XmlDtoConverter().Convert(metaData);

        converted.Should().BeEquivalentTo(expectedDto);
    }

    [Test]
    public void Convert_LuminaireMetaData_ShouldReturnCorrectSimpleDto()
    {
        var metaData = new Header();

        var expectedDto = new HeaderDto
        {
            CreationTimeCode = metaData.CreationTimeCode,
            CreatedWithApplication = string.Empty,
            Description = string.Empty,
            Name = string.Empty
        };

        var converted = new XmlDtoConverter().Convert(metaData);

        converted.Should().BeEquivalentTo(expectedDto);
    }

    [Test]
    public void Convert_Luminaire_ShouldReturnNull_WhenArgumentIsNull()
    {
        Luminaire luminaire = null;

        var converted = new XmlDtoConverter().Convert(luminaire);

        converted.Should().BeNull();
    }

    [Test]
    public void Convert_Luminaire_ShouldReturnCorrectDto()
    {
        var geometryDefinition = new GeometrySource
        {
            GeometryId = Guid.NewGuid().ToString(),
            Model = Substitute.For<IModel3D>(),
            Units = GeometricUnits.mm
        };
        var luminaire = new Luminaire()
        {
            GeometryDefinitions = { geometryDefinition },
            Parts =
            {
                new GeometryPart
                {
                    Name = Guid.NewGuid().ToString(),
                    GeometrySource = geometryDefinition
                }
            }
        };
        var converter = new XmlDtoConverter();
        var expectedDto = new LuminaireDto
        {
            GeometryDefinitions = luminaire.GeometryDefinitions.Select(definition => converter.Convert(definition)).ToList(),
            Parts = luminaire.Parts.Select(part => converter.Convert(part)).ToList(),
            Header = converter.Convert(luminaire.Header)
        };

        var converted = converter.Convert(luminaire);

        converted.Should().BeEquivalentTo(expectedDto, Config);
    }
}