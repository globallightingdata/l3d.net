using System;
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
    internal class JointOptionsTests
    {
        private class Context : IContextWithBuilder
        {
            public JointOptions Options { get; }
            public ILuminaireBuilder LuminaireBuilder { get; }
            public string KnownPartName { get; } = Guid.NewGuid().ToString();
            public JointPart KnownJointPart { get; }
            public ILogger Logger { get; }


            public Context()
            {
                LuminaireBuilder = Substitute.For<ILuminaireBuilder>();
                KnownJointPart = new JointPart(KnownPartName);
                Logger = LoggerSubstitute.Create();

                Options = new JointOptions(LuminaireBuilder, KnownJointPart, Logger);
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
            Action action = () =>
                new JointOptions(null, new JointPart(Guid.NewGuid().ToString()), Substitute.For<ILogger>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenSensorPartIsNull()
        {
            Action action = () =>
                new JointOptions(Substitute.For<ILuminaireBuilder>(), null, Substitute.For<ILogger>());

            action.Should().Throw<ArgumentNullException>();
        }
        

        [Test]
        public void ShouldSetDefaultRotation_Vector()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var expectedRotation = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options.WithDefaultRotation(expectedRotation);

            context.KnownJointPart.DefaultRotation.Should().BeEquivalentTo(expectedRotation);
        }

        [Test]
        public void ShouldSetDefaultRotation_Double()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var expectedRotattion = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options.WithDefaultRotation((double)expectedRotattion.X, expectedRotattion.Y, expectedRotattion.Z);

            context.KnownJointPart.DefaultRotation.Should().BeEquivalentTo(expectedRotattion);
        }

        [Test]
        public void ShouldSetDefaultRotation_Float()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var expectedRotattion = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            context.Options.WithDefaultRotation(expectedRotattion.X, expectedRotattion.Y, expectedRotattion.Z);

            context.KnownJointPart.DefaultRotation.Should().BeEquivalentTo(expectedRotattion);
        }

        [Test]
        public void ShouldSetXAxisDegreesOfFreedom()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var min = random.Next();
            var max = random.Next();
            var step = random.Next();

            context.Options.WithXAxisDegreesOfFreedom(min, max, step);

            context.KnownJointPart.XAxis.Min.Should().Be(min);
            context.KnownJointPart.XAxis.Max.Should().Be(max);
            context.KnownJointPart.XAxis.Step.Should().Be(step);
        }

        [Test]
        public void ShouldSetYAxisDegreesOfFreedom()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var min = random.Next();
            var max = random.Next();
            var step = random.Next();

            context.Options.WithYAxisDegreesOfFreedom(min, max, step);

            context.KnownJointPart.YAxis.Min.Should().Be(min);
            context.KnownJointPart.YAxis.Max.Should().Be(max);
            context.KnownJointPart.YAxis.Step.Should().Be(step);
        }

        [Test]
        public void ShouldSetZAxisDegreesOfFreedom()
        {
            var context = CreateContext();

            Random random = new Random(0);

            var min = random.Next();
            var max = random.Next();
            var step = random.Next();

            context.Options.WithZAxisDegreesOfFreedom(min, max, step);

            context.KnownJointPart.ZAxis.Min.Should().Be(min);
            context.KnownJointPart.ZAxis.Max.Should().Be(max);
            context.KnownJointPart.ZAxis.Step.Should().Be(step);
        }

        [Test]
        public void AddGeometry_ShouldCallBuilderThrowWhenPartNameIsInvalid()
        {
            var context = CreateContext();
            var partName = Guid.NewGuid().ToString();

            context.Options.AddGeometry(partName, Guid.NewGuid().ToString(), GeometricUnits.m);

            context.LuminaireBuilder.Received(1).ThrowWhenPartNameIsInvalid(partName);
        }

        [Test]
        [TestCase(GeometricUnits.m)]
        [TestCase(GeometricUnits.mm)]
        public void AddGeometry_ShouldCallBuilderEnsureGeometryFileDefinition(GeometricUnits units)
        {
            var context = CreateContext();
            var modelPath = Guid.NewGuid().ToString();

            context.Options.AddGeometry(Guid.NewGuid().ToString(), modelPath, units);

            context.LuminaireBuilder.Received(1).EnsureGeometryFileDefinition(modelPath, units);
        }
        
        [Test]
        public void AddGeometry_ShouldCreateNewGeometryNode()
        {
            var partName = Guid.NewGuid().ToString();
            var modelPath = Guid.NewGuid().ToString();
            GeometricUnits units = GeometricUnits.m;
            GeometryDefinition geometryDefinition = null;
            var context = CreateContext(options => options
                .WithGeometryDefinition(modelPath, units, out geometryDefinition)
            );
            GeometryPart expectedGeometryPart = new GeometryPart(partName, geometryDefinition);

            context.Options.AddGeometry(partName, modelPath, units);

            context.KnownJointPart.Geometries[0].Should().BeEquivalentTo(expectedGeometryPart);
        }

        [Test]
        public void AddGeometry_ShouldInvokeOptionsFunc_WhenNotNull()
        {
            var partName = Guid.NewGuid().ToString();
            var modelPath = Guid.NewGuid().ToString();
            GeometricUnits units = GeometricUnits.m;

            var context = CreateContext(options => options
                .WithGeometryDefinition(modelPath, units, out _)
            );
            
            Func<GeometryOptions, GeometryOptions> optionsFunc = Substitute.For<Func<GeometryOptions, GeometryOptions>>();

            context.Options.AddGeometry(partName, modelPath, units, optionsFunc);

            optionsFunc.Received(1);
        }
    }
}
