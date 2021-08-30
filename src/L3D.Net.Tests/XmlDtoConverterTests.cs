using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using FluentAssertions.Equivalency;
using L3D.Net.API.Dto;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_9_0;
using L3D.Net.XML.V0_9_0.Dto;
using NSubstitute;
using NUnit.Framework;
using CircleDto = L3D.Net.XML.V0_9_0.Dto.CircleDto;
using HeaderDto = L3D.Net.XML.V0_9_0.Dto.HeaderDto;
using LuminaireDto = L3D.Net.XML.V0_9_0.Dto.LuminaireDto;
using LuminousHeightsDto = L3D.Net.XML.V0_9_0.Dto.LuminousHeightsDto;
using RectangleDto = L3D.Net.XML.V0_9_0.Dto.RectangleDto;

// ReSharper disable ExpressionIsAlwaysNull

namespace L3D.Net.Tests
{
    [TestFixture]
    public class XmlDtoConverterTests
    {
        private readonly Random _random = new Random();

        private Vector3 NextVector3()
        {
            return new Vector3((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble());
        }

        private int NextPositiveInt()
        {
            return 1 + _random.Next(10000);
        }

        private EquivalencyAssertionOptions<T> Config<T>(EquivalencyAssertionOptions<T> options)
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

            Vector3 vector = new Vector3(x, y, z);

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

            var expectedDto = new AxisRotationTypeDto()
            {
                Min = min,
                Max = max,
                Step = step
            };

            AxisRotation vector = new AxisRotation(min, max, step);

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
            Shape shape = new Circle(diameter);
            var expectedDto = new CircleDto()
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
            Shape shape = new Rectangle(sizeX, sizeY);
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

            var luminousHeights = new LuminousHeights(c0, c90, c180, c270);

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

            var expectedDto = new SensorObjectDto
            {
                PartName = partName,
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

            var converted = new XmlDtoConverter().Convert(new SensorPart(partName)
            {
                Position = position,
                Rotation = rotation
            });

            converted.Should().BeEquivalentTo(expectedDto, Config);
        }

        [Test]
        public void Convert_LightEmittingFaceAssignment_ShouldReturnNull_WhenArgumentIsNull()
        {
            LightEmittingFaceAssignment assignment = null;

            var converted = new XmlDtoConverter().Convert(assignment);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_LightEmittingFaceAssignment_ShouldReturnCorrectDto_WhenArgumentIsSingleLightEmittingFaceAssignment()
        {
            var partName = Guid.NewGuid().ToString();
            var groupIndex = NextPositiveInt();
            var faceIndex = NextPositiveInt();
            var assignment = new SingleLightEmittingFaceAssignment(partName, groupIndex, faceIndex);
            var expectedDto = new AssignmentDto
            {
                LightEmittingPartName = partName,
                GroupIndex = groupIndex,
                FaceIndex = faceIndex
            };

            var converted = new XmlDtoConverter().Convert(assignment);

            converted.Should().BeEquivalentTo(expectedDto, Config);
        }

        [Test]
        public void Convert_LightEmittingFaceAssignment_ShouldReturnCorrectDto_WhenArgumentIsRangeLightEmittingFaceAssignment()
        {
            var partName = Guid.NewGuid().ToString();
            var groupIndex = NextPositiveInt();
            var faceIndexBegin = NextPositiveInt();
            var faceIndexEnd = faceIndexBegin + NextPositiveInt();
            var assignment = new LightEmittingFaceRangeAssignment(partName, groupIndex, faceIndexBegin, faceIndexEnd);
            var expectedDto = new RangeAssignmentDto
            {
                LightEmittingPartName = partName,
                GroupIndex = groupIndex,
                FaceIndexBegin = faceIndexBegin,
                FaceIndexEnd = faceIndexEnd
            };

            var converted = new XmlDtoConverter().Convert(assignment);

            converted.Should().BeEquivalentTo(expectedDto, Config);
        }

        class UnknownAssignement : LightEmittingFaceAssignment
        {
            public UnknownAssignement(string partName, int groupIndex) : base(partName, groupIndex)
            {
            }
        }

        [Test]
        public void Convert_LightEmittingFaceAssignment_ShouldThrowArgumentOutOfRangeException_WhenAssignmentTypeIsUnknown()
        {
            var partName = Guid.NewGuid().ToString();
            var groupIndex = NextPositiveInt();

            var assignment = new UnknownAssignement(partName, groupIndex);

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

            return new AxisRotation(min, max, step);
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

            var jointPart = new JointPart(jointPartName)
            {
                Position = position,
                Rotation = rotation,
                Geometries = { new GeometryPart(geomPartName, new GeometryDefinition(geomDefId, Substitute.For<IModel3D>(), GeometricUnits.m)) },
                DefaultRotation = defaultRotation,
                XAxis = xAxis,
                YAxis = yAxis,
                ZAxis = zAxis
            };

            var converter = new XmlDtoConverter();

            var expectedDto = new JointNodeDto
            {
                PartName = jointPartName,
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
                XAxis = new AxisRotationTypeDto
                {
                    Min = xAxis.Min,
                    Max = xAxis.Max,
                    Step = xAxis.Step
                },
                YAxis = new AxisRotationTypeDto
                {
                    Min = yAxis.Min,
                    Max = yAxis.Max,
                    Step = yAxis.Step
                },
                ZAxis = new AxisRotationTypeDto
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

            var jointPart = new JointPart(jointPartName)
            {
                Position = position,
                Rotation = rotation
            };

            var expectedDto = new JointNodeDto
            {
                PartName = jointPartName,
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
                Geometries = new List<GeometryNodeDto>()
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
            var shape = new Circle(0.0001 + _random.NextDouble());
            var position = NextVector3();
            var rotation = NextVector3();
            var luminousHeights = new LuminousHeights(_random.NextDouble(), _random.NextDouble(), _random.NextDouble(), _random.NextDouble());
            var lightEmittingPart = new LightEmittingPart(partName, shape)
            {
                Position = position,
                Rotation = rotation,
                LuminousHeights = luminousHeights
            };
            var expectedDto = new LightEmittingNodeDto
            {
                PartName = partName,
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
            var shape = new Rectangle(0.0001 + _random.NextDouble(), 0.0001 + _random.NextDouble());
            var lightEmittingPart = new LightEmittingPart(partName, shape);
            var expectedDto = new LightEmittingNodeDto
            {
                PartName = partName,
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
            var geometryDefinition = new GeometryDefinition(geomDefId, Substitute.For<IModel3D>(), GeometricUnits.m);
            var position = NextVector3();
            var rotation = NextVector3();
            var jointPart = new JointPart(Guid.NewGuid().ToString());
            var leoPart = new LightEmittingPart(Guid.NewGuid().ToString(), new Circle(0.0001 + _random.NextDouble()));
            var sensorPart = new SensorPart(Guid.NewGuid().ToString());
            var electricalConnector = NextVector3();
            var pendulumConnector = NextVector3();
            var assignment = new SingleLightEmittingFaceAssignment(Guid.NewGuid().ToString(), NextPositiveInt(), NextPositiveInt());
            var geometryPart = new GeometryPart(partName, geometryDefinition)
            {
                Position = position,
                Rotation = rotation,
                ExcludedFromMeasurement = true,
                Joints = { jointPart },
                LightEmittingObjects = { leoPart },
                Sensors = { sensorPart },
                ElectricalConnectors = { electricalConnector },
                PendulumConnectors = { pendulumConnector },
                LightEmittingFaceAssignments = { assignment }
            };

            var converter = new XmlDtoConverter();

            var expectedDto = new GeometryNodeDto
            {
                PartName = partName,
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
                ExcludedFromMeasurement = true,
                GeometrySource = new GeometryReferenceDto { GeometryId = geomDefId },
                Joints = geometryPart.Joints.Select(part => converter.Convert(part)).ToList(),
                LightEmittingObjects = geometryPart.LightEmittingObjects.Select(part => converter.Convert(part)).ToList(),
                SensorObjects = geometryPart.Sensors.Select(part => converter.Convert(part)).ToList(),
                LightEmittingFaceAssignments = geometryPart.LightEmittingFaceAssignments.Select(faceAssignment => converter.Convert(faceAssignment)).ToList(),
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
            var geometryDefinition = new GeometryDefinition(geomDefId, Substitute.For<IModel3D>(), GeometricUnits.m);
            var geometryPart = new GeometryPart(partName, geometryDefinition);

            var converter = new XmlDtoConverter();

            var expectedDto = new GeometryNodeDto
            {
                PartName = partName,
                Position = new Vector3Dto(),
                Rotation = new Vector3Dto(),
                ExcludedFromMeasurement = false,
                GeometrySource = new GeometryReferenceDto { GeometryId = geomDefId },
                Joints = new List<JointNodeDto>(),
                LightEmittingObjects = new List<LightEmittingNodeDto>(),
                SensorObjects = new List<SensorObjectDto>(),
                LightEmittingFaceAssignments = new List<AssignmentBaseDto>(),
                PendulumConnectors = new List<Vector3Dto>(),
                ElectricalConnectors = new List<Vector3Dto>(),
            };

            var converted = converter.Convert(geometryPart);

            converted.Should().BeEquivalentTo(expectedDto, Config);
        }

        [Test]
        public void Convert_GeometryDefinition_ShouldReturnNull_WhenArgumentIsNull()
        {
            GeometryDefinition geometryDefinition = null;

            var converted = new XmlDtoConverter().Convert(geometryDefinition);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_GeometryDefintion_ShouldReturnCorrectDto()
        {
            var id = Guid.NewGuid().ToString();
            var modelPath = Guid.NewGuid().ToString();
            var model = Substitute.For<IModel3D>();
            model.FilePath.Returns(modelPath);
            var geometryDefinition = new GeometryDefinition(id, model, GeometricUnits.m);
            var expectedDto = new GeometryFileDefinitionDto
            {
                Id = id,
                Filename = modelPath,
                Units = GeometryNodeUnits.m
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
                CreationTimeCode = metaData.CreationTimeCode
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
            var geometryDefinition = new GeometryDefinition(Guid.NewGuid().ToString(), Substitute.For<IModel3D>(), GeometricUnits.mm);
            var luminaire = new Luminaire()
            {
                GeometryDefinitions = { geometryDefinition },
                Parts = { new GeometryPart(Guid.NewGuid().ToString(), geometryDefinition) },
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
}
