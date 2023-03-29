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
        var action = () => new L3DXmlReader();
        action.Should().NotThrow();
    }
}