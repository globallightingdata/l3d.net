using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using L3D.Net.XML.V0_11_0;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Tests;

[TestFixture]
public class XmlDtoSerializerTests
{
    [SetUp]
    public void SetUp()
    {
        Setup.Initialize();
    }

    private static IEnumerable<Stream> ExampleFiles()
    {
        Setup.Initialize();
        return Setup.ExampleXmlStreams;
    }

    [Test]
    public void Serialize_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        var serializer = new XmlDtoSerializer();

        var action = () =>
        {
            using var ms = new MemoryStream();
            serializer.Serialize(null!, ms);
        };

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Serialize_ShouldThrowArgumentException_WhenStreamIsNullOrEmpty()
    {
        var serializer = new XmlDtoSerializer();

        var action = () => serializer.Serialize(new LuminaireDto(), null!);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCaseSource(nameof(ExampleFiles))]
    public void ExampleTest(Stream stream)
    {
        var serilizer = new XmlDtoSerializer();

        var dto = serilizer.Deserialize(stream);

        stream.Seek(0, SeekOrigin.Begin);

        using var ms = new MemoryStream();
        serilizer.Serialize(dto, ms);
        ms.Seek(0, SeekOrigin.Begin);

        var exampleDocument = XDocument.Load(stream);
        var testDocument = XDocument.Load(ms);

        testDocument.Should().BeEquivalentTo(exampleDocument);
    }
}