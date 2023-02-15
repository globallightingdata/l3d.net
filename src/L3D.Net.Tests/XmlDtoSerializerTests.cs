using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FluentAssertions;
using L3D.Net.XML.V0_9_2;
using L3D.Net.XML.V0_9_2.Dto;
using NUnit.Framework;

namespace L3D.Net.Tests;

[TestFixture]
public class XmlDtoSerializerTests
{
    private string _tempDirectory;

    [SetUp]
    public void Init()
    {
        Setup.Initialize();
        var tempFileName = Guid.NewGuid().ToString();
        _tempDirectory = Path.Combine(Path.GetTempPath(), tempFileName);

        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void Deinit()
    {
        Directory.Delete(_tempDirectory, true);
    }

    static IEnumerable<string> ExampleFiles()
    {
        Setup.Initialize();
        return Setup.ExampleXmlFiles;
    }

    [Test]
    public void Serialize_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        var serializer = new XmlDtoSerializer();

        Action action = () => serializer.Serialize(null, Guid.NewGuid().ToString());

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void Serialize_ShouldThrowArgumentException_WhenFilenameIsNullOrEmptyOrWhitespace(string filename)
    {
        var serializer = new XmlDtoSerializer();

        Action action = () => serializer.Serialize(new LuminaireDto(), filename);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCaseSource(nameof(ExampleFiles))]
    public void ExampleTest(string exampleXml)
    {
        var sourceFilename = Path.GetFileName(exampleXml);
        var targetFilepath = Path.Combine(_tempDirectory, sourceFilename);
        var serilizer = new XmlDtoSerializer();

        var dto = serilizer.Deserialize(exampleXml);

        serilizer.Serialize(dto, targetFilepath);

        var exampleDocument = XDocument.Load(exampleXml);
        var testDocument = XDocument.Load(targetFilepath);
            
        testDocument.Should().BeEquivalentTo(exampleDocument);
    }
}