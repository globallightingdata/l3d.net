using System;
using FluentAssertions;
using L3D.Net.XML;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests;

[TestFixture]
class L3dXmlReaderTests
{
    [Test]
    public void Constructor_ShouldNotThrowArgumentNullException_WhenLoggerIsNull()
    {
        Action action = () => new L3dXmlReader(null);
        action.Should().NotThrow();
    }
}