using System;
using System.Numerics;
using FluentAssertions;
using L3D.Net.BuilderOptions;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests;

class TransformableOptionsTests
{
    private class SimpleTransformable : TransformablePart
    {
        public SimpleTransformable(string name) : base(name)
        {
        }
    }

    private class SimpleTransformableOptions : TransformableOptions
    {
        internal SimpleTransformableOptions(ILuminaireBuilder builder, TransformablePart data) : base(builder, data, Substitute.For<ILogger>())
        {
        }
    }

    private class Context
    {
        public ILuminaireBuilder LuminaireBuilder { get; }
        public SimpleTransformable Part { get; }
        public SimpleTransformableOptions Options { get; }
            

        public Context()
        {
            LuminaireBuilder = Substitute.For<ILuminaireBuilder>();
            Part = new SimpleTransformable(Guid.NewGuid().ToString());
            Options = new SimpleTransformableOptions(LuminaireBuilder, Part);
        }
    }

    private Context CreateContext()
    {
        var context = new Context();

        return context;
    }

    [Test]
    public void TransformableOptions_ShouldSetPosition_Vector()
    {
        var context = CreateContext();

        Random random = new Random(0);

        var vector0 = new Vector3((float) random.NextDouble(), (float) random.NextDouble(),
            (float) random.NextDouble());

        context.Options.WithPosition(vector0);

        context.Part.Position.Should().Be(vector0);
    }

    [Test]
    public void TransformableOptions_ShouldSetPosition_Double()
    {
        var context = CreateContext();

        Random random = new Random(0);

        var vector0 = new Vector3((float) random.NextDouble(), (float) random.NextDouble(),
            (float) random.NextDouble());

        context.Options.WithPosition((double) vector0.X, vector0.Y, vector0.Z);

        context.Part.Position.Should().Be(vector0);
    }

    [Test]
    public void TransformableOptions_ShouldSetPosition_Float()
    {
        var context = CreateContext();

        Random random = new Random(0);

        var vector0 = new Vector3((float) random.NextDouble(), (float) random.NextDouble(),
            (float) random.NextDouble());

        context.Options.WithPosition(vector0.X, vector0.Y, vector0.Z);

        context.Part.Position.Should().Be(vector0);
    }

    [Test]
    public void TransformableOptions_ShouldSetRotation_Vector()
    {
        var context = CreateContext();

        Random random = new Random(0);

        var vector0 = new Vector3((float) random.NextDouble(), (float) random.NextDouble(),
            (float) random.NextDouble());

        context.Options.WithRotation(vector0);

        context.Part.Rotation.Should().Be(vector0);
    }

    [Test]
    public void TransformableOptions_ShouldSetRotation_Double()
    {
        var context = CreateContext();

        Random random = new Random(0);

        var vector0 = new Vector3((float) random.NextDouble(), (float) random.NextDouble(),
            (float) random.NextDouble());

        context.Options.WithRotation((double) vector0.X, vector0.Y, vector0.Z);

        context.Part.Rotation.Should().Be(vector0);
    }

    [Test]
    public void TransformableOptions_ShouldSetRotation_Float()
    {
        var context = CreateContext();

        Random random = new Random(0);

        var vector0 = new Vector3((float) random.NextDouble(), (float) random.NextDouble(),
            (float) random.NextDouble());

        context.Options.WithRotation(vector0.X, vector0.Y, vector0.Z);

        context.Part.Rotation.Should().Be(vector0);
    }
}