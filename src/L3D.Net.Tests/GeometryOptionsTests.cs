using System;
using System.Linq;
using System.Numerics;
using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.API.Dto;
using L3D.Net.BuilderOptions;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using L3D.Net.Tests.Context;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests
{
    [TestFixture]
    public class GeometryOptionsTests
    {
        private class Context : IContextWithBuilder
        {
            public ILuminaireBuilder LuminaireBuilder { get; }
            public ILogger Logger { get; }
            public string KnownGeometryDefinitionId { get; }
            public GeometricUnits KnownGeometryUnits { get; }
            public GeometryDefinition KnownGeometryDefinition { get; }
            public string KnownGeometryPartName { get; }
            public GeometryPart KnownGeometryPart { get; }
            public GeometryOptions Options { get; }

            public Context()
            {
                LuminaireBuilder = Substitute.For<ILuminaireBuilder>();
                Logger = LoggerSubstitute.Create();

                KnownGeometryDefinitionId = Guid.NewGuid().ToString();
                KnownGeometryUnits = GeometricUnits.m;
                KnownGeometryDefinition = new GeometryDefinition(KnownGeometryDefinitionId, Substitute.For<IModel3D>(), KnownGeometryUnits);
                KnownGeometryPartName = Guid.NewGuid().ToString();
                KnownGeometryPart = new GeometryPart(KnownGeometryPartName, KnownGeometryDefinition);

                Options = new GeometryOptions(LuminaireBuilder, KnownGeometryPart, Logger);
            }
        }

        private class ContextOptions : IContextWithBuilderOptions
        {
            private readonly Context _context;

            public ContextOptions(Context context)
            {
                _context = context;
            }

            IContextWithBuilder IContextWithBuilderOptions.Context => _context;

            public void WithValidGeometryDefinitionModel()
            {
                _context.KnownGeometryDefinition.Model.IsFaceIndexValid(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
            }

            public void WithValidGeometryDefinitionModelFace(int faceIndex, int groupIndex)
            {
                _context.KnownGeometryDefinition.Model.IsFaceIndexValid(groupIndex, faceIndex).Returns(true);
            }
        }

        private Context CreateContext(Action<ContextOptions> options = null)
        {
            var context = new Context();
            options?.Invoke(new ContextOptions(context));
            return context;
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenBuilderIsNull()
        {
            var geometryPart = new GeometryPart(Guid.NewGuid().ToString(), new GeometryDefinition(Guid.NewGuid().ToString(), Substitute.For<IModel3D>(), GeometricUnits.m));
            Action action = () => new GeometryOptions(null, geometryPart, Substitute.For<ILogger>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenSensorPartIsNull()
        {
            Action action = () => new GeometryOptions(Substitute.For<ILuminaireBuilder>(), null, Substitute.For<ILogger>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddPendulumConnector_ShouldAddPendulumConnector_Vector()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var vector0 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            var vector1 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options
                .WithPendulumConnector(vector0)
                .WithPendulumConnector(vector1);

            context.KnownGeometryPart.PendulumConnectors[0].Should().Be(vector0);
            context.KnownGeometryPart.PendulumConnectors[1].Should().Be(vector1);
        }

        [Test]
        public void AddPendulumConnector_ShouldAddPendulumConnector_Double()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var vector0 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            var vector1 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options
                .WithPendulumConnector((double)vector0.X, vector0.Y, vector0.Z)
                .WithPendulumConnector((double)vector1.X, vector1.Y, vector1.Z);

            context.KnownGeometryPart.PendulumConnectors[0].Should().Be(vector0);
            context.KnownGeometryPart.PendulumConnectors[1].Should().Be(vector1);
        }

        [Test]
        public void AddPendulumConnector_ShouldAddPendulumConnector_Float()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var vector0 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            var vector1 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options
                .WithPendulumConnector(vector0.X, vector0.Y, vector0.Z)
                .WithPendulumConnector(vector1.X, vector1.Y, vector1.Z);

            context.KnownGeometryPart.PendulumConnectors[0].Should().Be(vector0);
            context.KnownGeometryPart.PendulumConnectors[1].Should().Be(vector1);
        }

        [Test]
        public void AddElectricalConnector_ShouldAddElectricalConnector_Vector()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var vector0 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            var vector1 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options
                .WithElectricalConnector(vector0)
                .WithElectricalConnector(vector1);

            context.KnownGeometryPart.ElectricalConnectors[0].Should().Be(vector0);
            context.KnownGeometryPart.ElectricalConnectors[1].Should().Be(vector1);
        }

        [Test]
        public void AddElectricalConnector_ShouldAddElectricalConnector_Double()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var vector0 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            var vector1 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options
                .WithElectricalConnector((double)vector0.X, vector0.Y, vector0.Z)
                .WithElectricalConnector((double)vector1.X, vector1.Y, vector1.Z);

            context.KnownGeometryPart.ElectricalConnectors[0].Should().Be(vector0);
            context.KnownGeometryPart.ElectricalConnectors[1].Should().Be(vector1);
        }

        [Test]
        public void ShouldAddElectricalConnector_ShouldAddElectricalConnector_Float()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var vector0 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            var vector1 = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options
                .WithElectricalConnector(vector0.X, vector0.Y, vector0.Z)
                .WithElectricalConnector(vector1.X, vector1.Y, vector1.Z);

            context.KnownGeometryPart.ElectricalConnectors[0].Should().Be(vector0);
            context.KnownGeometryPart.ElectricalConnectors[1].Should().Be(vector1);
        }


        [Test]
        public void AddJoint_ShouldCallBuilderThrowWhenPartNameIsInvalid()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();

            context.Options.AddJoint(partName);

            context.LuminaireBuilder.Received(1).ThrowWhenPartNameIsInvalid(partName);
        }

        [Test]
        public void AddJoint_ShouldAddJoint()
        {
            var partName = Guid.NewGuid().ToString();
            var context = CreateContext();
            var expectedJointPart = new JointPart(partName);
            context.Options.AddJoint(partName);

            context.KnownGeometryPart.Joints[0].Should().BeEquivalentTo(expectedJointPart);
        }

        [Test]
        public void AddLightEmittingObject_Circle_ShouldCallBuilderThrowWhenPartNameIsInvalid()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();

            context.Options.AddCircularLightEmittingObject(partName, 0.1);

            context.LuminaireBuilder.Received(1).ThrowWhenPartNameIsInvalid(Arg.Is(partName));
        }

        [Test]
        public void AddLightEmittingObject_Rectangle_ShouldCallBuilderThrowWhenPartNameIsInvalid()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();

            context.Options.AddRectangularLightEmittingObject(partName, 0.1, 0.1);
            
            context.LuminaireBuilder.Received(1).ThrowWhenPartNameIsInvalid(Arg.Is(partName));
        }

        [Test]
        public void AddLightEmittingObject_Circle_ShouldCallDataFactoryCreateLightEmittingObject_WithCylinderShape()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();
            var diameter = 0.123;
            context.Options.AddCircularLightEmittingObject(partName, diameter);

            var expectedLightEmittingPart = new LightEmittingPart(partName, new Circle(diameter));
            
            context.Options.Data.LightEmittingObjects.Should().HaveCount(1).And.Subject.First().Should().BeEquivalentTo(expectedLightEmittingPart);
        }

        [Test]
        public void AddLightEmittingObject_Rectangle_ShouldCallDataFactoryCreateLightEmittingObject_WithCorrectParameters()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();
            var sizeX = 0.1234;
            var sizeY = 0.5678;
            context.Options.AddRectangularLightEmittingObject(partName, sizeX, sizeY);

            var expectedLightEmittingPart = new LightEmittingPart(partName, new Rectangle(sizeX, sizeY));
            
            context.Options.Data.LightEmittingObjects.Should().HaveCount(1).And.Subject.First().Should().BeEquivalentTo(expectedLightEmittingPart);
        }

        [Test]
        public void AddLightEmittingObject_Circle_ShouldAddLightEmittingObject()
        {
            var partName = Guid.NewGuid().ToString();
            var diameter = 0.1234;
            var context = CreateContext();

            context.Options.AddCircularLightEmittingObject(partName, diameter);

            var lightEmittingObject = context.KnownGeometryPart.LightEmittingObjects[0];
            lightEmittingObject.Name.Should().Be(partName);
            lightEmittingObject.Shape.Should().BeOfType<Circle>().Which.Diameter.Should().Be(diameter);
        }

        [Test]
        public void AddLightEmittingObject_Rectangle_ShouldAddLightEmittingObject()
        {
            var partName = Guid.NewGuid().ToString();
            var context = CreateContext();
            var sizeX = 0.1234;
            var sizeY = 0.5678;
            context.Options.AddRectangularLightEmittingObject(partName, sizeX, sizeY);

            var lightEmittingObject = context.KnownGeometryPart.LightEmittingObjects[0];
            lightEmittingObject.Name.Should().Be(partName);
            lightEmittingObject.Shape.Should().BeOfType<Rectangle>().Which.SizeX.Should().Be(sizeX);
            lightEmittingObject.Shape.Should().BeOfType<Rectangle>().Which.SizeY.Should().Be(sizeY);
        }

        [Test]
        public void WithLightEmittingSurface_ShouldThrowArgumentException_WhenLeoPartNameIsUnknown()
        {
            var context = CreateContext();

            Action action = () => context.Options.WithLightEmittingSurface(Guid.NewGuid().ToString(), 1, 2);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void WithLightEmittingSurface_ShouldCallLoggerLogWithWarning_WhenFaceIndexOrGroupIndexAreInvalid()
        {
            var partName = Guid.NewGuid().ToString();
            var context = CreateContext(options => options
                .WithValidLightEmittingPartName(partName)
            );

            context.Options
                .AddCircularLightEmittingObject(partName, 0.1)
                .WithLightEmittingSurface(partName, 10, 20);

            context.Logger.Received(1).Log(LogLevel.Warning, "The given groupIndex(20)/faceIndex(10) combination is not valid!");
        }

        [Test]
        public void WithLightEmittingSurface_ShouldThrowArgumentException_WhenLeoPartNameIsNotLightEmittingObject()
        {
            var context = CreateContext();

            var partName = Guid.NewGuid().ToString();

            Action action = () => context.Options.WithLightEmittingSurface(partName, 1, 2);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void WithLightEmittingSurfaces_ShouldThrowArgumentException_WhenLeoPartNameIsUnknown()
        {
            var context = CreateContext();

            Action action = () => context.Options.WithLightEmittingSurfaces(Guid.NewGuid().ToString(), 1, 2, 3);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void WithLightEmittingSurfaces_ShouldThrowArgumentException_WhenLeoPartNameIsNotLightEmittingObject()
        {
            var context = CreateContext();

            var partName = Guid.NewGuid().ToString();

            Action action = () => context.Options.WithLightEmittingSurfaces(partName, 1, 2, 3);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void WithLightEmittingSurfaces_ShouldCallLoggerOnWarning_WhenFaceIndexBeginOrGroupIndexAreInvalid()
        {
            var partName = Guid.NewGuid().ToString();
            var context = CreateContext(options => options
                .WithValidLightEmittingPartName(partName)
            );

            context.Options
                .AddCircularLightEmittingObject(partName, 0.1)
                .WithLightEmittingSurfaces(partName, 10, 20, 30);

            context.Logger.Received(1).LogWarning(@"The given groupIndex(30)/faceIndexBegin(10) combination is not valid!");
        }

        [Test]
        public void WithLightEmittingSurfaces_ShouldCallLoggerLogWithWarning_WhenFaceIndexEndOrGroupIndexAreInvalid()
        {
            var partName = Guid.NewGuid().ToString();
            var context = CreateContext(options => options
                .WithValidLightEmittingPartName(partName)
                .WithValidGeometryDefinitionModelFace(10, 30)
            );

            context.Options
                .AddCircularLightEmittingObject(partName, 0.1)
                .WithLightEmittingSurfaces(partName, 10, 20, 30);

            context.Logger.Received(1).LogWarning(@"The given groupIndex(30)/faceIndexEnd(20) combination is not valid!");
        }

        [Test]
        public void WithLightEmittingSurface_ShouldAddLightEmittingSurface_WithCorrectParameters()
        {
            var partName = Guid.NewGuid().ToString();
            var context = CreateContext(options => options
                .WithValidLightEmittingPartName(partName)
                .WithValidGeometryDefinitionModel()
            );
            var expectedAssignement = new SingleLightEmittingFaceAssignment(partName, 1, 2);

            context.Options
                .AddCircularLightEmittingObject(partName, 0.1)
                .WithLightEmittingSurface(partName, expectedAssignement.FaceIndex, expectedAssignement.GroupIndex);

            context.KnownGeometryPart.LightEmittingFaceAssignments[0]
                .Should().BeOfType<SingleLightEmittingFaceAssignment>().Which
                .Should().BeEquivalentTo(expectedAssignement);
        }

        [Test]
        public void WithLightEmittingSurfaces_ShouldAddLightEmittingSurfaceRange_WithCorrectParameters()
        {
            var partName = Guid.NewGuid().ToString();
            var context = CreateContext(options => options
                .WithValidLightEmittingPartName(partName)
                .WithValidGeometryDefinitionModel()
            );
            var expectedAssignement = new LightEmittingFaceRangeAssignment(partName, 1, 2, 3);

            context.Options
                .AddCircularLightEmittingObject(expectedAssignement.PartName, 0.1)
                .WithLightEmittingSurfaces(expectedAssignement.PartName, expectedAssignement.FaceIndexBegin, expectedAssignement.FaceIndexEnd, expectedAssignement.GroupIndex);

            context.KnownGeometryPart.LightEmittingFaceAssignments[0]
                .Should().BeOfType<LightEmittingFaceRangeAssignment>().Which
                .Should().BeEquivalentTo(expectedAssignement);
        }

        [Test]
        public void WithLightEmittingSurfaces_ShouldAddSingleLightEmittingSurfaceAssignment_WhenFaceIndexBeginAndFaceIndexEndAreaEqual()
        {
            var partName = Guid.NewGuid().ToString();
            var context = CreateContext(options => options
                .WithValidLightEmittingPartName(partName)
                .WithValidGeometryDefinitionModel()
            );
            var expectedAssignement = new SingleLightEmittingFaceAssignment(partName, 1, 2);

            context.Options
                .AddCircularLightEmittingObject(expectedAssignement.PartName, 0.1)
                .WithLightEmittingSurfaces(expectedAssignement.PartName, expectedAssignement.FaceIndex, expectedAssignement.FaceIndex, expectedAssignement.GroupIndex);

            context.KnownGeometryPart.LightEmittingFaceAssignments[0]
                .Should().BeOfType<SingleLightEmittingFaceAssignment>().Which
                .Should().BeEquivalentTo(expectedAssignement);
        }

        [Test]
        public void AddSensorObject_ShouldCallBuilderThrowWhenPartNameIsInvalid()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();

            context.Options.AddSensorObject(partName);

            context.LuminaireBuilder.Received(1).ThrowWhenPartNameIsInvalid(Arg.Is(partName));
        }

        [Test]
        public void AddSensorObject_ShouldCreateSensorPart()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();

            context.Options.AddSensorObject(partName);

            context.Options.Data.Sensors.Should().HaveCount(1);
        }

        [Test]
        public void AddSensorObject_ShouldAddSensorPart()
        {
            var partName = Guid.NewGuid().ToString();
            SensorPart expectedSensorPart = new SensorPart(partName);
            var context = CreateContext();

            context.Options.AddSensorObject(partName);

            context.KnownGeometryPart.Sensors[0].Should().BeEquivalentTo(expectedSensorPart);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AddGeometryOptions_ShouldSetExcludedFromMeasurement(bool isExcluded)
        {
            var context = CreateContext();

            context.Options.WithExcludedFromMeasurement(isExcluded);

            context.KnownGeometryPart.ExcludedFromMeasurement.Should().Be(isExcluded);
        }

    }
}
