using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using L3D.Net.API;
using L3D.Net.API.Dto;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ExpressionIsAlwaysNull

namespace L3D.Net.Tests
{
    [TestFixture]
    public class ApiDtoConverterTests
    {
        private readonly Random _random = new Random();

        private int GetPositiveInt()
        {
            return 1 + _random.Next(1000000);
        }

        private double GetPositiveDouble()
        {
            return 0.001 + _random.NextDouble();
        }

        private Vector3 GetVector()
        {
            return new((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble());
        }

        private string GetStringOrNull()
        {
            if (_random.Next(2) == 0)
                return null;

            return Guid.NewGuid().ToString();
        }

        private class Context
        {
            public ApiDtoConverter Converter { get; }
            public IFileHandler FileHandler { get; }

            public Context()
            {
                FileHandler = Substitute.For<IFileHandler>();
                Converter = new ApiDtoConverter(FileHandler);
            }
        }

        private Context CreateContext()
        {
            return new Context();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenFileHandlerIsNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new ApiDtoConverter(null);
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Test]
        public void Convert_Shape_ShouldReturnNull_WhenShapeIsNull()
        {
            var context = CreateContext();
            
            Shape shape = null;

            var converted = context.Converter.Convert(shape);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_Shape_ShouldReturnCorrectDto_WhenShapeIsCircle()
        {
            var context = CreateContext();
            var diameter = GetPositiveDouble();
            Shape shape = new Circle(diameter);
            var expectedDto = new CircleDto { Diameter = diameter };

            var converted = context.Converter.Convert(shape);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_Shape_ShouldReturnCorrectDto_WhenShapeIsRectangle()
        {
            var context = CreateContext();
            var sizeX = GetPositiveDouble();
            var sizeY = GetPositiveDouble();
            Shape shape = new Rectangle(sizeX, sizeY);
            var expectedDto = new RectangleDto { SizeX = sizeX, SizeY = sizeY };

            var converted = context.Converter.Convert(shape);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        private class InvalidShape : Shape
        {
        };

        [Test]
        public void Convert_Shape_ShouldThrowArgumentOutOfRangeException_WhenShapeTypeIsNotKnown()
        {
            var context = CreateContext();
            Action action = () => context.Converter.Convert(new InvalidShape());

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void Convert_LuminousHeights_ShouldReturnNull_WhenLuminousHeightsIsNull()
        {
            var context = CreateContext();
            LuminousHeights luminousHeights = null;

            var converted = context.Converter.Convert(luminousHeights);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_LuminousHeights_ShouldReturnCorrectDto()
        {
            var context = CreateContext();
            var c0 = GetPositiveDouble();
            var c90 = GetPositiveDouble();
            var c180 = GetPositiveDouble();
            var c270 = GetPositiveDouble();
            var luminousHeights = new LuminousHeights(c0, c90, c180, c270);
            var expectedDto = new LuminousHeightsDto { C0 = c0, C90 = c90, C180 = c180, C270 = c270 };

            var converted = context.Converter.Convert(luminousHeights);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_SensorPart_ShouldReturnNull_WhenSensorPartIsNull()
        {
            var context = CreateContext();
            SensorPart sensorPart = null;

            var converted = context.Converter.Convert(sensorPart);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_SensorPart_ShouldReturnCorrectDto()
        {
            var context = CreateContext();
            var name = Guid.NewGuid().ToString();
            var position = GetVector();
            var rotation = GetVector();
            var sensorPart = new SensorPart(name)
            {
                Position = position,
                Rotation = rotation
            };
            var expectedDto = new SensorPartDto
            {
                Name = name,
                Position = position,
                Rotation = rotation
            };

            var converted = context.Converter.Convert(sensorPart);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_LightEmittingSurface_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            LightEmittingSurfacePart les = null;
            var converted = context.Converter.Convert(les);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_LightEmittingSurface_ShouldReturnCorrectDto()
        {
            var lesPartName = Guid.NewGuid().ToString();
            var leo0PartName = Guid.NewGuid().ToString();
            var leo1PartName = Guid.NewGuid().ToString();
            
            var context = CreateContext();
            var les = new LightEmittingSurfacePart(lesPartName);
            les.AddLightEmittingObject(leo0PartName, 0.3);
            les.AddLightEmittingObject(leo1PartName);
            les.AddFaceAssignment(1, 2);
            les.AddFaceAssignment(3, 6, 9);


            var expectedLightEmittingSurfaceDto = new LightEmittingSurfacePartDto
            {
                Name = lesPartName,
                Position = new Vector3(),
                Rotation = new Vector3(),
                LightEmittingObjects = new Dictionary<string, double>
                {
                    [leo0PartName] = 0.3,
                    [leo1PartName] = 1.0
                },
                FaceAssignments = new List<BaseAssignmentDto>
                {
                    new SingleFaceAssignmentDto { GroupIndex = 1, FaceIndex = 2 },
                    new RangeFaceAssignmentDto { GroupIndex = 3, FaceIndexBegin = 6, FaceIndexEnd = 9 }
                }
            };

            var converted = context.Converter.Convert(les);

            converted.Should().BeEquivalentTo(expectedLightEmittingSurfaceDto);
        }
        
        [Test]
        public void Convert_LightEmittingFaceAssignment_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            FaceAssignment assignment = null;

            var converted = context.Converter.Convert(assignment);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_LightEmittingFaceAssignment_ShouldReturnCorrectDto_WhenAssignmentIsSingle()
        {
            var context = CreateContext();
            var groupIndex = GetPositiveInt();
            var faceIndex = GetPositiveInt();
            var assignemnt = new SingleFaceAssignment(groupIndex, faceIndex);
            var expectedDto = new SingleFaceAssignmentDto
            {
                GroupIndex = groupIndex,
                FaceIndex = faceIndex
            };

            var converted = context.Converter.Convert(assignemnt);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_LightEmittingFaceAssignment_ShouldReturnCorrectDto_WhenAssignmentIsRange()
        {
            var context = CreateContext();
            var groupIndex = GetPositiveInt();
            var faceIndexBegin = GetPositiveInt();
            var faceIndexEnd = faceIndexBegin + GetPositiveInt();

            var assignment = new FaceRangeAssignment(groupIndex, faceIndexBegin, faceIndexEnd);
            
            var expectedDto = new RangeFaceAssignmentDto
            {
                GroupIndex = groupIndex,
                FaceIndexBegin = faceIndexBegin,
                FaceIndexEnd = faceIndexEnd
            };

            var converted = context.Converter.Convert(assignment);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        private void AddLightEmittingRangeAssignment(LightEmittingSurfacePart les)
        {
            var groupIndex = GetPositiveInt();
            var faceIndexBegin = GetPositiveInt();
            var faceIndexEnd = faceIndexBegin + GetPositiveInt();
            
            les.AddFaceAssignment(groupIndex, faceIndexBegin, faceIndexEnd);
        }

        private class InvalidAssignment : FaceAssignment
        {
            public InvalidAssignment(int groupIndex) : base(groupIndex)
            {
            }
        };

        [Test]
        public void Convert_LightEmittingFaceAssignment_ShouldThrowArgumentOutOfRangeException_WhenAssignmentTypeIsNotKnown()
        {
            var context = CreateContext();
            Action action = () => context.Converter.Convert(new InvalidAssignment(1));

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void Convert_LightEmittingPart_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            LightEmittingPart part = null;

            var converted = context.Converter.Convert(part);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_LightEmittingPart_ShouldReturnCorrectDto_WhenSimple()
        {
            var context = CreateContext();
            var name = Guid.NewGuid().ToString();
            var shape = new Circle(GetPositiveDouble());
            var lightEmittingPart = new LightEmittingPart(name, shape);
            var converter = context.Converter;
            var expectedDto = new LightEmittingPartDto
            {
                Name = name,
                Shape = converter.Convert(shape)
            };

            var converted = converter.Convert(lightEmittingPart);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_LightEmittingPart_ShouldReturnCorrectDto_WhenComplex()
        {
            var context = CreateContext();
            var name = Guid.NewGuid().ToString();
            var shape = new Circle(GetPositiveDouble());
            var position = GetVector();
            var rotation = GetVector();
            var luminousHeights = new LuminousHeights(GetPositiveDouble(), GetPositiveDouble(), GetPositiveDouble(), GetPositiveDouble());
            var lightEmittingPart = new LightEmittingPart(name, shape)
            {
                Position = position,
                Rotation = rotation,
                LuminousHeights = luminousHeights
            };
            var converter = context.Converter;
            var expectedDto = new LightEmittingPartDto
            {
                Name = name,
                Shape = converter.Convert(shape),
                Position = position,
                Rotation = rotation,
                LuminousHeights = converter.Convert(luminousHeights)
            };

            var converted = converter.Convert(lightEmittingPart);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_AxisRotation_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            AxisRotation axisRotation = null;

            var converted = context.Converter.Convert(axisRotation);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_AxisRotation_ShouldReturnCorrectDto()
        {
            var context = CreateContext();
            var min = _random.NextDouble();
            var max = min + _random.NextDouble();
            var step = GetPositiveDouble();
            var axisRotation = new AxisRotation(min, max, step);
            var expectedDto = new AxisRotationDto
            {
                Min = min,
                Max = max,
                Step = step
            };

            var converted = context.Converter.Convert(axisRotation);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_JointPart_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            JointPart jointPart = null;

            var converted = context.Converter.Convert(jointPart, new List<GeometryDefinitionDto>());

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_JointPart_ShouldThrowArgumentNullException_WhenKnownGeometryDefinitionListIsNull()
        {
            var context = CreateContext();
            Action action = () => context.Converter.Convert(new JointPart(Guid.NewGuid().ToString()), null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Convert_JointPart_ShouldReturnCorrectDto_WhenSimple()
        {
            var context = CreateContext();
            var name = Guid.NewGuid().ToString();
            var jointPart = new JointPart(name);
            var geometryDefinitionDtos = new List<GeometryDefinitionDto>();
            var expectedDto = new JointPartDto
            {
                Name = name,
                Geometries = new List<GeometryPartDto>()
            };

            var converted = context.Converter.Convert(jointPart, geometryDefinitionDtos);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_JointPart_ShouldReturnCorrectDto_WhenComplex()
        {
            var context = CreateContext();
            var name = Guid.NewGuid().ToString();
            var defaultRotation = new Vector3((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble());
            var position = GetVector();
            var rotation = GetVector();

            var xAxis = new AxisRotation(GetPositiveDouble(), 1 + GetPositiveDouble(), GetPositiveDouble());
            var yAxis = new AxisRotation(GetPositiveDouble(), 1 + GetPositiveDouble(), GetPositiveDouble());
            var zAxis = new AxisRotation(GetPositiveDouble(), 1 + GetPositiveDouble(), GetPositiveDouble());

            var geomDefinition = CreateGeometryDefinition();
            var geomPartName = Guid.NewGuid().ToString();
            var geomPart = new GeometryPart(geomPartName, geomDefinition);
            var jointPart = new JointPart(name)
            {
                DefaultRotation = defaultRotation,
                Position = position,
                Rotation = rotation,
                XAxis = xAxis,
                YAxis = yAxis,
                ZAxis = zAxis,
                Geometries = { geomPart }
            };

            var geometryDefinitionDtos = new List<GeometryDefinitionDto>();
            var converter = context.Converter;
            var geomPartDto = converter.Convert(geomDefinition, Guid.NewGuid().ToString());
            geometryDefinitionDtos.Add(geomPartDto);
            var expectedDto = new JointPartDto
            {
                Name = name,
                Geometries = new List<GeometryPartDto> { converter.Convert(geomPart, geometryDefinitionDtos) },
                DefaultRotation = defaultRotation,
                Position = position,
                Rotation = rotation,
                XAxis = converter.Convert(xAxis),
                YAxis = converter.Convert(yAxis),
                ZAxis = converter.Convert(zAxis)
            };


            var converted = context.Converter.Convert(jointPart, geometryDefinitionDtos);


            converted.Should().BeEquivalentTo(expectedDto);
        }

        private static GeometryDefinition CreateGeometryDefinition()
        {
            var geomDefId = Guid.NewGuid().ToString();
            var geomDefModel = Substitute.For<IModel3D>();
            geomDefModel.Data.Returns(_ => new ModelData(Enumerable.Empty<Vector3>(), Enumerable.Empty<Vector3>(),
                Enumerable.Empty<Vector2>(), Enumerable.Empty<ModelFaceGroup>(), Enumerable.Empty<ModelMaterial>()));
            var geomDefinition = new GeometryDefinition(geomDefId, geomDefModel, GeometricUnits.m);
            return geomDefinition;
        }

        [Test]
        public void Convert_GeometryPart_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            GeometryPart part = null;

            var converted = context.Converter.Convert(part, new List<GeometryDefinitionDto>());

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_GeometryPart_ShouldThrowArgumentNullException_WhenKnownGeometryDefinitionListIsNull()
        {
            var context = CreateContext();
            var geometryDefinition = CreateGeometryDefinition();

            Action action = () => context.Converter.Convert(new GeometryPart(Guid.NewGuid().ToString(), geometryDefinition), null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Convert_GeometryPart_ShouldReturnCorrectDto_WhenSimple()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();
            var geometryDefinition = CreateGeometryDefinition();
            var converter = context.Converter;
            var geometryPart = new GeometryPart(partName, geometryDefinition);
            var geometryDefinitionDto = converter.Convert(geometryDefinition, Guid.NewGuid().ToString());
            var expectedDto = new GeometryPartDto
            {
                Name = partName,
                Position = new Vector3(0, 0, 0),
                Rotation = new Vector3(0, 0, 0),
                ExcludedFromMeasurement = false,
                GeometryDefinition = geometryDefinitionDto,
                Joints = new List<JointPartDto>(),
                LightEmittingObjects = new List<LightEmittingPartDto>(),
                Sensors = new List<SensorPartDto>(),
                ElectricalConnectors = new List<Vector3>(),
                PendulumConnectors = new List<Vector3>(),
                LightEmittingSurfaces = new List<LightEmittingSurfacePartDto>()
            };

            var converted = converter.Convert(geometryPart, new List<GeometryDefinitionDto> { geometryDefinitionDto });

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_GeometryPart_ShouldThrowArgumentException_WhenKnownGeometryDefinitionListHasNoMatchingEntry()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();
            var geometryDefinition = CreateGeometryDefinition();
            var converter = context.Converter;
            var geometryPart = new GeometryPart(partName, geometryDefinition);

            Action action = () => converter.Convert(geometryPart, new List<GeometryDefinitionDto>());

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Convert_GeometryPart_ShouldReturnCorrectDto_WhenComplex()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();
            var position = GetVector();
            var rotation = GetVector();
            var joints = new List<JointPart> { new JointPart(Guid.NewGuid().ToString()), new JointPart(Guid.NewGuid().ToString()) };
            var leos = new List<LightEmittingPart>
            {
                new LightEmittingPart(Guid.NewGuid().ToString(), new Circle(GetPositiveDouble())),
                new LightEmittingPart(Guid.NewGuid().ToString(), new Rectangle(GetPositiveDouble(), GetPositiveDouble()))
            };
            var lefas = new List<LightEmittingSurfacePart>
            {
                new LightEmittingSurfacePart(Guid.NewGuid().ToString()),
                new LightEmittingSurfacePart(Guid.NewGuid().ToString())
            };
            
            lefas[0].AddFaceAssignment(_random.Next(), _random.Next());
            AddLightEmittingRangeAssignment(lefas[0]);
                
            var sensors = new List<SensorPart> { new SensorPart(Guid.NewGuid().ToString()), new SensorPart(Guid.NewGuid().ToString()) };
            var electircalConnectors = new List<Vector3> { GetVector(), GetVector() };
            var pendulumConnectors = new List<Vector3> { GetVector(), GetVector() };
            var geometryDefinition = CreateGeometryDefinition();
            var converter = context.Converter;
            var geometryDefinitionDto = converter.Convert(geometryDefinition, Guid.NewGuid().ToString());
            var knownGeometryDefinitions = new List<GeometryDefinitionDto> { geometryDefinitionDto };
            var geometryPart = new GeometryPart(partName, geometryDefinition)
            {
                Position = position,
                Rotation = rotation,
                ExcludedFromMeasurement = true,
                Joints = { joints[0], joints[1] },
                LightEmittingObjects = { leos[0], leos[1] },
                LightEmittingSurfaces = { lefas[0], lefas[1] },
                Sensors = { sensors[0], sensors[1] },
                ElectricalConnectors = { electircalConnectors[0], electircalConnectors[1] },
                PendulumConnectors = { pendulumConnectors[0], pendulumConnectors[1] }
            };
            var expectedDto = new GeometryPartDto
            {
                Name = partName,
                Position = position,
                Rotation = rotation,
                ExcludedFromMeasurement = true,
                GeometryDefinition = geometryDefinitionDto,
                Joints = joints.Select(part => converter.Convert(part, knownGeometryDefinitions)).ToList(),
                LightEmittingObjects = leos.Select(part => converter.Convert(part)).ToList(),
                LightEmittingSurfaces = lefas.Select(assignment => converter.Convert(assignment)).ToList(),
                Sensors = sensors.Select(part => converter.Convert(part)).ToList(),
                ElectricalConnectors = electircalConnectors,
                PendulumConnectors = pendulumConnectors
            };

            var converted = converter.Convert(geometryPart, knownGeometryDefinitions);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_ModelFaceVertex_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            ModelFaceVertex vertex = null;

            var converted = context.Converter.Convert(vertex);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_ModelFaceVertex_ShouldReturnCorrectDto_WhenIndexIsPositive()
        {
            var context = CreateContext();
            var vertex = new ModelFaceVertex(GetPositiveInt(), GetPositiveInt(), GetPositiveInt());
            var expectedDto = new FaceVertexDto
            {
                VertexIndex = vertex.VertexIndex,
                NormalIndex = vertex.NormalIndex,
                TextureCoordinateIndex = vertex.TextureCoordinateIndex
            };

            var converted = context.Converter.Convert(vertex);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_ModelFaceVertex_ShouldReturnCorrectDto_WhenIndexIsNegative()
        {
            var context = CreateContext();
            var vertex = new ModelFaceVertex(-GetPositiveInt(), -GetPositiveInt(), -GetPositiveInt());
            var expectedDto = new FaceVertexDto
            {
                VertexIndex = vertex.VertexIndex,
                NormalIndex = vertex.NormalIndex,
                TextureCoordinateIndex = vertex.TextureCoordinateIndex
            };

            var converted = context.Converter.Convert(vertex);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_ModelFace_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            ModelFace modelFace = null;

            var converted = context.Converter.Convert(modelFace);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_ModelFace_ShouldReturnCorrect()
        {
            var context = CreateContext();
            var materialIndex = GetPositiveInt();
            var vertices = CreateVertices();
            var face = new ModelFace(vertices, materialIndex);
            var converter = context.Converter;
            var expectedDto = new FaceDto
            {
                MaterialIndex = materialIndex,
                Vertices = vertices.Select(vertex => converter.Convert(vertex)).ToList()
            };

            var converted = context.Converter.Convert(face);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        private List<ModelFaceVertex> CreateVertices()
        {
            var vertices = Enumerable.Range(0, _random.Next(5, 20))
                .Select(_ => new ModelFaceVertex(GetPositiveInt(), GetPositiveInt(), GetPositiveInt())).ToList();
            return vertices;
        }

        [Test]
        public void Convert_ModelPart_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            ModelFaceGroup modelFaceGroup = null;

            var converted = context.Converter.Convert(modelFaceGroup);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_ModelPart_ShouldReturnCorrect()
        {
            var context = CreateContext();
            var name = Guid.NewGuid().ToString();
            var faces = CreateFaces();
            var part = new ModelFaceGroup(name, faces);
            var converter = context.Converter;
            var expectedDto = new FaceGroupDto
            {
                Name = name,
                Faces = faces.Select(face => converter.Convert(face)).ToList()
            };

            var converted = context.Converter.Convert(part);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        private List<ModelFace> CreateFaces()
        {
            var faces = Enumerable.Range(0, _random.Next(5, 20)).Select(_ => new ModelFace(CreateVertices(), GetPositiveInt()))
                .ToList();
            return faces;
        }

        [Test]
        public void Convert_Model_ShouldReturnNull_WhenModelIsNull()
        {
            var context = CreateContext();
            IModel3D model3D = null;

            var converted = context.Converter.Convert(model3D, Guid.NewGuid().ToString(), 1.0, Guid.NewGuid().ToString());

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_Model_ShouldThrowArgumentException_WhenGeomIdIsNull()
        {
            var context = CreateContext();
            IModel3D model3D = Substitute.For<IModel3D>();

            Action action = () => context.Converter.Convert(model3D, null, 1.0, Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void Convert_Model_ShouldThrowArgumentException_WhenDirectoryScopeIsNullOrEmpty(string directory)
        {
            var context = CreateContext();
            IModel3D model3D = Substitute.For<IModel3D>();

            Action action = () => context.Converter.Convert(model3D, Guid.NewGuid().ToString(), 1.0, directory);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Convert_Model_ShouldThrowArgumentException_WhenScaleIsZero()
        {
            var context = CreateContext();
            Action action = () => context.Converter.Convert(Substitute.For<IModel3D>(), Guid.NewGuid().ToString(), 0, Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase(0.5)]
        [TestCase(1.0)]
        [TestCase(1.5)]
        public void Convert_Model_ShouldReturnCorrectScaledDto(double scale)
        {
            var context = CreateContext();
            var vertices = Enumerable.Range(5, 20).Select(_ => GetVector()).ToList();
            var normals = Enumerable.Range(5, 20).Select(_ => GetVector()).ToList();
            var textCorrds = Enumerable.Range(5, 20).Select(_ => new Vector2((float)_random.NextDouble(), (float)_random.NextDouble())).ToList();
            var parts = Enumerable.Range(1, 5).Select(_ => new ModelFaceGroup(Guid.NewGuid().ToString(), CreateFaces())).ToList();
            var materials = Enumerable.Range(1, 5).Select(_ => new ModelMaterial(Guid.NewGuid().ToString(), GetVector(), GetStringOrNull())).ToList();

            var modelData = new ModelData(vertices, normals, textCorrds, parts, materials);
            var model = Substitute.For<IModel3D>();
            model.Data.Returns(modelData);

            var converter = context.Converter;
            var expectedDto = new ModelDto
            {
                Vertices = vertices.Select(vert => vert * (float)scale).ToArray(),
                Normals = normals.ToArray(),
                TextureCoordinates = textCorrds.ToArray(),
                FaceGroups = parts.Select(part => converter.Convert(part)).ToArray(),
                Materials = materials.Select(material => converter.Convert(material, Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).ToArray()
            };

            var converted = converter.Convert(model, Guid.NewGuid().ToString(), scale, Guid.NewGuid().ToString());

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_GeometryDefinition_ShouldReturnNull_WhenDefinitionIsNull()
        {
            var context = CreateContext();
            GeometryDefinition geometryDefinition = null;

            var converted = context.Converter.Convert(geometryDefinition, Guid.NewGuid().ToString());

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_GeometryDefinition_ShouldThrowArgumentNullException_WhenDirectoryScopeIsNull()
        {
            var context = CreateContext();
            GeometryDefinition geometryDefinition = new GeometryDefinition(Guid.NewGuid().ToString(), Substitute.For<IModel3D>(), GeometricUnits.m);

            Action action = () => context.Converter.Convert(geometryDefinition, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Convert_GeometryDefinition_ShouldReturnCorrectDto_WhenUnitIs_m()
        {
            var context = CreateContext();
            var vertices = Enumerable.Range(5, 20).Select(_ => GetVector()).ToList();
            var normals = Enumerable.Range(5, 20).Select(_ => GetVector()).ToList();
            var textCorrds = Enumerable.Range(5, 20).Select(_ => new Vector2((float)_random.NextDouble(), (float)_random.NextDouble())).ToList();
            var parts = Enumerable.Range(1, 5).Select(_ => new ModelFaceGroup(Guid.NewGuid().ToString(), CreateFaces())).ToList();
            var materials = Enumerable.Range(1, 5).Select(_ => new ModelMaterial(Guid.NewGuid().ToString(), GetVector(), GetStringOrNull())).ToList();

            var modelData = new ModelData(vertices, normals, textCorrds, parts, materials);
            var model = Substitute.For<IModel3D>();
            model.Data.Returns(modelData);

            var geomId = Guid.NewGuid().ToString();
            var geometryDefinition = new GeometryDefinition(geomId, model, GeometricUnits.m);
            var converter = context.Converter;
            var expectedDto = new GeometryDefinitionDto
            {
                Id = geomId,
                Model = converter.Convert(model, Guid.NewGuid().ToString(), 1.0, Guid.NewGuid().ToString())
            };

            var converted = converter.Convert(geometryDefinition, Guid.NewGuid().ToString());

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_GeometryDefinition_ShouldReturnCorrectDto_WhenUnitIs_mm()
        {
            var context = CreateContext();
            var vertices = Enumerable.Range(5, 20).Select(_ => GetVector()).ToList();
            var normals = Enumerable.Range(5, 20).Select(_ => GetVector()).ToList();
            var textCorrds = Enumerable.Range(5, 20).Select(_ => new Vector2((float)_random.NextDouble(), (float)_random.NextDouble())).ToList();
            var parts = Enumerable.Range(1, 5).Select(_ => new ModelFaceGroup(Guid.NewGuid().ToString(), CreateFaces())).ToList();
            var materials = Enumerable.Range(1, 5).Select(_ => new ModelMaterial(Guid.NewGuid().ToString(), GetVector(), GetStringOrNull())).ToList();

            var modelData = new ModelData(vertices, normals, textCorrds, parts, materials);
            var model = Substitute.For<IModel3D>();
            model.Data.Returns(modelData);

            var geomId = Guid.NewGuid().ToString();
            var geometryDefinition = new GeometryDefinition(geomId, model, GeometricUnits.mm);
            var converter = context.Converter;
            var expectedDto = new GeometryDefinitionDto
            {
                Id = geomId,
                Model = converter.Convert(model, Guid.NewGuid().ToString(), 1.0 / 1000.0, Guid.NewGuid().ToString())
            };

            var converted = converter.Convert(geometryDefinition, Guid.NewGuid().ToString());

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_Header_ShouldReturnNull_WhenArgumentIsNull()
        {
            var context = CreateContext();
            Header header = null;

            var converted = context.Converter.Convert(header);

            converted.Should().BeNull();
        }

        [Test]
        public void Convert_Header_ShouldReturnCorrectDto_WhenSimple()
        {
            var context = CreateContext();
            var header = new Header();
            var expectedDto = new HeaderDto
            {
                CreationTimeCode = header.CreationTimeCode,
                CreatedWithApplication = null,
                Name = null,
                Description = null
            };

            var converted = context.Converter.Convert(header);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_Header_ShouldReturnCorrectDto_WhenComplex()
        {
            var context = CreateContext();
            var header = new Header
            {
                CreatedWithApplication = Guid.NewGuid().ToString(),
                CreationTimeCode = DateTime.UtcNow,
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString()
            };
            var expectedDto = new HeaderDto
            {
                CreationTimeCode = header.CreationTimeCode,
                CreatedWithApplication = header.CreatedWithApplication,
                Name = header.Name,
                Description = header.Description
            };

            var converted = context.Converter.Convert(header);

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_Luminaire_ShouldReturnNull_WhenLuminaireIsNull()
        {
            var context = CreateContext();
            Luminaire luminaire = null;

            var converted = context.Converter.Convert(luminaire, Guid.NewGuid().ToString());

            converted.Should().BeNull();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void Convert_Luminaire_ShouldThrowArgumentNullException_WhenDirectoryScopeIsNullOrEmpty(string directory)
        {
            var context = CreateContext();
            Luminaire luminaire = new Luminaire();

            Action action = () => context.Converter.Convert(luminaire, directory);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Convert_Luminaire_ShouldReturnCorrectDto_WhenComplex()
        {
            var context = CreateContext();
            var geometryDefinitions = new List<GeometryDefinition> { CreateGeometryDefinition(), CreateGeometryDefinition() };
            var parts = new List<GeometryPart>
            {
                new(Guid.NewGuid().ToString(), geometryDefinitions[0]),
                new(Guid.NewGuid().ToString(), geometryDefinitions[1])
            };
            var luminaire = new Luminaire
            {
                Header =
                {
                    CreatedWithApplication = Guid.NewGuid().ToString(),
                    CreationTimeCode = DateTime.UtcNow,
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString()
                },
                GeometryDefinitions = { geometryDefinitions[0], geometryDefinitions[1] },
                Parts = { parts[0], parts[1] }
            };
            var converter = context.Converter;
            var geometryDefinitionDtos = geometryDefinitions.Select(definition => converter.Convert(definition, Guid.NewGuid().ToString())).ToList();
            var expectedDto = new LuminaireDto
            {
                Header = converter.Convert(luminaire.Header),
                GeometryDefinitions = geometryDefinitionDtos,
                Parts = parts.Select(part => converter.Convert(part, geometryDefinitionDtos)).ToList()
            };

            var converted = converter.Convert(luminaire, Guid.NewGuid().ToString());

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_Luminaire_ShouldReturnCorrectDto_WhenSimple()
        {
            var context = CreateContext();
            var luminaire = new Luminaire();
            var converter = context.Converter;
            var expectedDto = new LuminaireDto
            {
                Header = converter.Convert(luminaire.Header),
                GeometryDefinitions = new List<GeometryDefinitionDto>(),
                Parts = new List<GeometryPartDto>()
            };

            var converted = converter.Convert(luminaire, Guid.NewGuid().ToString());

            converted.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public void Convert_Material_ShouldReturnNull_WhenMaterialIsNull()
        {
            var context = CreateContext();
            ModelMaterial material = null;

            var converter = context.Converter;

            var converted = 
                converter.Convert(material, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            converted.Should().BeNull();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void Convert_Material_ShouldThrowArgumentException_WhenGeomIdIsNullOrEmpty(string geomId)
        {
            var context = CreateContext();
            var converter = context.Converter;
            var material = new ModelMaterial(Guid.NewGuid().ToString(), GetVector(), GetStringOrNull());

            Action action = () =>
                converter.Convert(material, geomId, Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentException>();
        }
        
        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void Convert_Material_ShouldThrowArgumentNullException_WhenDirectoryScopeIsNullOrEmpty(string directory)
        {
            var context = CreateContext();
            var material = new ModelMaterial(Guid.NewGuid().ToString(), GetVector(), GetStringOrNull());

            Action action = () =>
                context.Converter.Convert(material, Guid.NewGuid().ToString(), directory);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Convert_Material_ShouldReturnCorrectDto()
        {
            var context = CreateContext();
            var converter = context.Converter;

            var modelMaterial = new ModelMaterial(Guid.NewGuid().ToString(), GetVector(), Guid.NewGuid().ToString());
            var geomId = Guid.NewGuid().ToString();
            var directoryScope = Guid.NewGuid().ToString();
            var bytes = new byte[] { 0, 1, 2, 3 };
            context.FileHandler
                .GetTextureBytes(Arg.Is(directoryScope), Arg.Is(geomId), Arg.Is(modelMaterial.TextureName))
                .Returns(bytes);
            
            var expected = new MaterialDto
            {
                Name = modelMaterial.Name,
                Color = modelMaterial.Color,
                TextureName = modelMaterial.TextureName,
                TextureBytes = bytes.ToArray()
            };
                
            var converted = converter.Convert(modelMaterial, geomId, directoryScope);
            
            converted.Should().BeEquivalentTo(expected);
        }
    }
}
