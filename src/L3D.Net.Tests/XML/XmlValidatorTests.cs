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
    private XmlValidator _xmlValidator = null!;

    [SetUp]
    public void SetUp()
    {
        Setup.Initialize();
        _xmlValidator = new XmlValidator();
    }

    private static IEnumerable<string> GetXmlFiles(string testDirectory)
    {
        Setup.Initialize();
        var directory = Path.Combine(Setup.TestDataDirectory, "xml", "validation", testDirectory);
        return Directory.EnumerateFiles(directory, "*.xml");
    }

    private static IEnumerable<TestCaseData> GenerateXmlTestCases(string testDirectory) => GenerateXmlTestCases(GetXmlFiles(testDirectory));

    private static IEnumerable<TestCaseData> GenerateXmlTestCases(IEnumerable<string> files)
        => files.Select(file => new TestCaseData(file).SetArgDisplayNames(file.Replace(Setup.TestDataDirectory, "", StringComparison.Ordinal)));

    public static IEnumerable<TestCaseData> GetNoRootTestFiles() => GenerateXmlTestCases("no_root");

    [Test]
    [TestCaseSource(nameof(GetNoRootTestFiles))]
    public void ValidateFile_ShouldCreateValidationHintWithExpectedMessage_WhenXmlHasNoRoot(string testFile)
    {
        using var fs = File.OpenRead(testFile);
        var result = _xmlValidator.ValidateStream(fs);

        result.Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlNotReadable);
    }

    private static IEnumerable<TestCaseData> GetNoLocationTestFiles() => GenerateXmlTestCases("no_scheme_location");

    [Test]
    [TestCaseSource(nameof(GetNoLocationTestFiles))]
    public void ValidateFile_ShouldCreateValidationHintWithExpectedMessage_WhenSchemeLocationIsMissing(string testFile)
    {
        using var fs = File.OpenRead(testFile);
        var result = _xmlValidator.ValidateStream(fs);

        result.Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlVersionMissing);
    }

    private static IEnumerable<TestCaseData> GetUnknownSchemeTestFiles() => GenerateXmlTestCases("unknown_scheme");

    [Test]
    [TestCaseSource(nameof(GetUnknownSchemeTestFiles))]
    public void ValidateFile_ShouldCreateValidationHintWithExpectedMessage_WhenSchemeIsNotKnown(string testFile)
    {
        using var fs = File.OpenRead(testFile);
        var result = _xmlValidator.ValidateStream(fs);

        result.Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlVersionNotReadable);
    }

    private static IEnumerable<TestCaseData> GetInvalidTestFiles() => GenerateXmlTestCases("invalid").Concat(GenerateXmlTestCases(Setup.InvalidVersionXmlFiles));

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void ValidateFile_ShouldCreateValidationHints_WhenXmlIsInvalid(string testFile)
    {
        using var fs = File.OpenRead(testFile);
        var result = _xmlValidator.ValidateStream(fs);

        result.Should().NotBeEmpty();
    }

    private static IEnumerable<TestCaseData> GetValidTestFiles() => GenerateXmlTestCases(Setup.ExampleXmlFiles.Concat(Setup.ValidVersionXmlFiles));

    [Test]
    [TestCaseSource(nameof(GetValidTestFiles))]
    public void ValidateFile_ShouldReturnNoValidationHints_WhenXmlIsValid(string testFile)
    {
        using var fs = File.OpenRead(testFile);
        var result = _xmlValidator.ValidateStream(fs);

        result.Should().BeEmpty();
    }
}