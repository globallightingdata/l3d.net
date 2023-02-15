using System;
using FluentAssertions;
using L3D.Net.BuilderOptions;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests;

[TestFixture]
public class SensorOptionsTest
{

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenBuilderIsNull()
    {
        Action action = () =>
            new SensorOptions(null, new SensorPart(Guid.NewGuid().ToString()), Substitute.For<ILogger>());

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenSensorPartIsNull()
    {
        Action action = () =>
            new SensorOptions(Substitute.For<ILuminaireBuilder>(), null, Substitute.For<ILogger>());

        action.Should().Throw<ArgumentNullException>();
    }
}