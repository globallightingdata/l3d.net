using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.XML;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace L3D.Net.Tests;

[TestFixture]
public class XmlValidatorTests
{
    private static IEnumerable<string> GetXmlFiles(string testDirectory)
    {
        var directory = Path.Combine(Setup.TestDataDirectory, "xml", "validation", testDirectory);
        return Directory.EnumerateFiles(directory, "*.xml").ToList();
    }

    public static IEnumerable<string> GetNoRootTestFiles() => GetXmlFiles("no_root");

    [Test]
    [TestCaseSource(nameof(GetNoRootTestFiles))]
    public void ValidateFile_ShouldThrow_WhenXmlHasNoRoot(string testFile)
    {
        var xmlValidator = new XmlValidator();

        var action = () =>
        {
            using var fs = File.OpenRead(testFile);
            return xmlValidator.ValidateStream(fs);
        };

        action.Should().Throw<Exception>().WithMessage("Root element is missing.");
    }

    private static IEnumerable<string> GetNoLocationTestFiles() => GetXmlFiles("no_scheme_location");

    [Test]
    [TestCaseSource(nameof(GetNoLocationTestFiles))]
    public void ValidateFile_ShouldThrow_WhenSchemeLocationIsMissing(string testFile)
    {
        var xmlValidator = new XmlValidator();

        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().BeFalse();
    }

    private static IEnumerable<string> GetUnknownSchemeTestFiles() => GetXmlFiles("unknown_scheme");

    [Test]
    [TestCaseSource(nameof(GetUnknownSchemeTestFiles))]
    public void ValidateFile_ShouldThrow_WhenSchemeIsNotKnown(string testFile)
    {
        var xmlValidator = new XmlValidator();

        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().BeFalse();
    }

    static IEnumerable<string> GetInvalidTestFiles()
    {
        Setup.Initialize();
        return GetXmlFiles("invalid").Union(Setup.InvalidVersionXmlFiles);
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void ValidateFile_ShouldReturnFalse_WhenXmlIsInvalid(string testFile)
    {
        var xmlValidator = new XmlValidator();

        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().BeFalse();
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void ValidateFile_ShouldCallLoggerLogWithError_WhenXmlIsInvalid(string testFile)
    {
        var xmlValidator = new XmlValidator();
        var logger = LoggerSubstitute.Create();
        using var fs = File.OpenRead(testFile);
        xmlValidator.ValidateStream(fs, logger);

        logger.Received(1).LogError(Arg.Any<string>());
    }

    private static IEnumerable<string> GetValidTestFiles()
    {
        Setup.Initialize();
        return Setup.ExampleXmlFiles.Union(Setup.ValidVersionXmlFiles);
    }

    [Test]
    [TestCaseSource(nameof(GetValidTestFiles))]
    public void ValidateFile_ShouldReturnTrue_WhenXmlIsValid(string testFile)
    {
        var xmlValidator = new XmlValidator();

        using var fs = File.OpenRead(testFile);
        var result = xmlValidator.ValidateStream(fs);

        result.Should().BeTrue();
    }
}