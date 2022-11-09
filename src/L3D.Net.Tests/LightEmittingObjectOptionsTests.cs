using System;
using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.BuilderOptions;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests
{
    [TestFixture]
    public class LightEmittingObjectOptionsTests
    {
        class Context
        {
            public string KnownPartName { get; } = Guid.NewGuid().ToString();
            public LightEmittinObjectOptions Options { get; }
            public ILuminaireBuilder LuminaireBuilder { get; }
            public LightEmittingPart KnownLightEmittingPart { get; }
            public ILightEmittingSurfaceHolder LightEmittingSurfaceHolder { get; }
            public ILogger Logger { get; }

            public Context()
            {
                LuminaireBuilder = Substitute.For<ILuminaireBuilder>();
                LightEmittingSurfaceHolder = Substitute.For<ILightEmittingSurfaceHolder>();
                Logger = LoggerSubstitute.Create();

                KnownLightEmittingPart = new LightEmittingPart(KnownPartName, new Circle(0.1));

                Options = new LightEmittinObjectOptions(LuminaireBuilder, KnownLightEmittingPart,
                    LightEmittingSurfaceHolder, Logger);
            }
        }

        private Context CreateContext()
        {
            return new Context();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenBuilderIsNull()
        {
            Action action = () => new LightEmittinObjectOptions(null,
                new LightEmittingPart(Guid.NewGuid().ToString(), new Circle(0.1)),
                Substitute.For<ILightEmittingSurfaceHolder>(), Substitute.For<ILogger>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenLightEmittingPartIsNull()
        {
            Action action = () => new LightEmittinObjectOptions(Substitute.For<ILuminaireBuilder>(),
                null, Substitute.For<ILightEmittingSurfaceHolder>(),
                Substitute.For<ILogger>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenLightEmittingSurfaceHolderIsNull()
        {
            Action action = () => new LightEmittinObjectOptions(Substitute.For<ILuminaireBuilder>(),
                new LightEmittingPart(Guid.NewGuid().ToString(), new Circle(0.1)), null,
                Substitute.For<ILogger>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void WithLuminousHeights_ShouldSetLuminousHeights()
        {
            var context = CreateContext();
            var c0 = 0.12;
            var c90 = 0.34;
            var c180 = 0.56;
            var c270 = 0.78;

            context.Options.WithLuminousHeights(c0, c90, c180, c270);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            context.KnownLightEmittingPart.LuminousHeights.Should().NotBeNull().And
                .Match<LuminousHeights>(heights =>
                    heights.C0 == c0 &&
                    heights.C90 == c90 &&
                    heights.C180 == c180 &&
                    heights.C270 == c270);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        [Test]
        public void WithLightEmittingSurface_ShouldCallLightEmittingSurfaceHolderCreateLightEmittingSurface()
        {
            var context = CreateContext();

            context.Options.WithLightEmittingSurfaceOnParent(context.KnownPartName, lesOptions => lesOptions);

            context.LightEmittingSurfaceHolder.Received(1)
                .WithLightEmittingSurface(context.KnownPartName, Arg.Any<Action<ILightEmittingSurfaceOptions>>());
        }

        [Test]
        public void WithLightEmittingSurfaces_ShouldCallLightEmittingSurfaceHolderCreateLightEmittingSurfaces()
        {
            var context = CreateContext();

            context.Options.WithLightEmittingSurfaceOnParent(context.KnownPartName, lesOptions => lesOptions);

            context.LightEmittingSurfaceHolder.Received(1)
                .WithLightEmittingSurface(context.KnownPartName, Arg.Any<Action<ILightEmittingSurfaceOptions>>());
        }
    }
}