using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.XML;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests;

[TestFixture]
public class XmlValidatorTests
{
    static IEnumerable<string> GetXmlFiles(string testDirectory)
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

        Action action = () => xmlValidator.ValidateFile(testFile, null);

        action.Should().Throw<Exception>().WithMessage("Root element is missing.");
    }

    static IEnumerable<string> GetNoLocationTestFiles() => GetXmlFiles("no_scheme_location");

    [Test]
    [TestCaseSource(nameof(GetNoLocationTestFiles))]
    public void ValidateFile_ShouldThrow_WhenSchemeLocationIsMissing(string testFile)
    {
        var xmlValidator = new XmlValidator();

        Action action = () => xmlValidator.ValidateFile(testFile, null);

        action.Should().Throw<Exception>()
            .WithMessage(
                @"XML document does not reference a valid XSD scheme in namespace (http://www.w3.org/2001/XMLSchema-instance)!");
    }

    static IEnumerable<string> GetUnknownSchemeTestFiles() => GetXmlFiles("unknown_scheme");

    [Test]
    [TestCaseSource(nameof(GetUnknownSchemeTestFiles))]
    public void ValidateFile_ShouldThrow_WhenSchemeIsNotKnown(string testFile)
    {
        var xmlValidator = new XmlValidator();

        Action action = () => xmlValidator.ValidateFile(testFile, null);

        action.Should().Throw<Exception>()
            .WithMessage("The scheme (*) is not known! Try update the L3D.Net component and try again.");
    }

    static IEnumerable<string> GetInvalidTestFiles() => GetXmlFiles("invalid");

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void ValidateFile_ShouldReturnFalse_WhenXmlIsInvalid(string testFile)
    {
        var xmlValidator = new XmlValidator();

        var result = xmlValidator.ValidateFile(testFile, null);

        result.Should().BeFalse();
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void ValidateFile_ShouldCallLoggerLogWithError_WhenXmlIsInvalid(string testFile)
    {
        var xmlValidator = new XmlValidator();
        var logger = LoggerSubstitute.Create();
        xmlValidator.ValidateFile(testFile, logger);

        logger.Received(1).LogError(Arg.Any<string>());
    }

    static IEnumerable<string> GetValidTestFiles() => Setup.ExampleXmlFiles;

    [Test]
    [TestCaseSource(nameof(GetValidTestFiles))]
    public void ValidateFile_ShouldReturnTrue_WhenXmlIsValid(string testFile)
    {
        var xmlValidator = new XmlValidator();

        var result = xmlValidator.ValidateFile(testFile, null);

        result.Should().BeTrue();
    }
}