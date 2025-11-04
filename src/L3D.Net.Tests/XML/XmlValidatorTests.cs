using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using L3D.Net.Abstract;
using L3D.Net.XML;
using NUnit.Framework;

namespace L3D.Net.Tests.XML;

[TestFixture]
public class XmlValidatorTests
{
    private static IEnumerable<string> GetXmlFiles(string testDirectory)
    {
        Setup.Initialize();
        var directory = Path.Combine(Setup.TestDataDirectory, "xml", "validation", testDirectory);
        return Directory.EnumerateFiles(directory, "*.xml").ToList();
    }

    public static IEnumerable<string> GetNoRootTestFiles() => GetXmlFiles("no_root");

    [Test]
    [TestCaseSource(nameof(GetNoRootTestFiles))]
    public void ValidateFile_ShouldCreateValidationHintWithExpectedMessage_WhenXmlHasNoRoot(string testFile)
    {
        var xmlValidator = new XmlValidator();

        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlNotReadable);
    }

    private static IEnumerable<string> GetNoLocationTestFiles() => GetXmlFiles("no_scheme_location");

    [Test]
    [TestCaseSource(nameof(GetNoLocationTestFiles))]
    public void ValidateFile_ShouldCreateValidationHintWithExpectedMessage_WhenSchemeLocationIsMissing(string testFile)
    {
        var xmlValidator = new XmlValidator();

        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlVersionMissing);
    }

    private static IEnumerable<string> GetUnknownSchemeTestFiles() => GetXmlFiles("unknown_scheme");

    [Test]
    [TestCaseSource(nameof(GetUnknownSchemeTestFiles))]
    public void ValidateFile_ShouldCreateValidationHintWithExpectedMessage_WhenSchemeIsNotKnown(string testFile)
    {
        var xmlValidator = new XmlValidator();

        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlVersionNotReadable);
    }

    private static IEnumerable<string> GetInvalidTestFiles()
    {
        Setup.Initialize();
        return GetXmlFiles("invalid").Concat(Setup.InvalidVersionXmlFiles);
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void ValidateFile_ShouldCreateValidationHints_WhenXmlIsInvalid(string testFile)
    {
        var xmlValidator = new XmlValidator();
        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().NotBeEmpty();
    }

    private static IEnumerable<TestCaseData> GetValidTestFiles()
    {
        Setup.Initialize();
        foreach (var file in Setup.ExampleXmlFiles.Concat(Setup.ValidVersionXmlFiles))
        {
            yield return new TestCaseData(file)
                .SetArgDisplayNames(file.Replace(Setup.TestDataDirectory, "", StringComparison.Ordinal));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetValidTestFiles))]
    public void ValidateFile_ShouldReturnNoValidationHints_WhenXmlIsValid(string testFile)
    {
        var xmlValidator = new XmlValidator();

        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().BeEmpty();
    }
}