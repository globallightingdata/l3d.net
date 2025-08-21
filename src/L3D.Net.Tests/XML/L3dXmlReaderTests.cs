using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests.XML;

[TestFixture]
public class L3dXmlReaderTests
{
    [Test]
    public void Constructor_ShouldNotThrowArgumentNullException_WhenLoggerIsNull()
    {
        var action = () => new L3DXmlReader();
        action.Should().NotThrow();
    }

    [Test]
    public void Read_ShouldThrow_WhenXmlStreamIsNull()
    {
        var l3DXmlReader = new L3DXmlReader();

        var act = () => l3DXmlReader.Read(new ContainerCache());
        act.Should().Throw<ArgumentException>();
    }

    private static IEnumerable<string> GetXmlFiles(string testDirectory)
    {
        Setup.Initialize();
        var directory = Path.Combine(Setup.TestDataDirectory, "xml", "validation", testDirectory);
        return Directory.EnumerateFiles(directory, "*.xml").ToList();
    }

    public static IEnumerable<TestCaseData> GetNoRootTestFiles()
    {
        foreach (var xmlFile in GetXmlFiles("no_root"))
        {
            yield return new TestCaseData(xmlFile).SetArgDisplayNames(xmlFile.Replace(Setup.TestDataDirectory, "", StringComparison.Ordinal));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetNoRootTestFiles))]
    public void Read_ShouldThrow_WhenXmlHasNoRoot(string testFile)
    {
        var l3DXmlReader = new L3DXmlReader();

        var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        using (cache)
        {
            var act = () => l3DXmlReader.Read(cache);
            act.Should().Throw<InvalidL3DException>();
        }
    }

    private static IEnumerable<TestCaseData> GetNoLocationTestFiles()
    {
        foreach (var xmlFile in GetXmlFiles("no_scheme_location"))
        {
            yield return new TestCaseData(xmlFile).SetArgDisplayNames(xmlFile.Replace(Setup.TestDataDirectory, "", StringComparison.Ordinal));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetNoLocationTestFiles))]
#pragma warning disable S4144 // Methods should not have identical implementations
    public void Read_ShouldThrow_WhenSchemeLocationIsMissing(string testFile)
#pragma warning restore S4144 // Methods should not have identical implementations
    {
        var l3DXmlReader = new L3DXmlReader();

        var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        using (cache)
        {
            var act = () => l3DXmlReader.Read(cache);
            act.Should().Throw<InvalidL3DException>();
        }
    }

    private static IEnumerable<TestCaseData> GetUnknownSchemeTestFiles()
    {
        foreach (var se in GetXmlFiles("unknown_scheme"))
        {
            yield return new TestCaseData(se).SetArgDisplayNames(se.Replace(Setup.TestDataDirectory, "", StringComparison.Ordinal));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetUnknownSchemeTestFiles))]
#pragma warning disable S4144 // Methods should not have identical implementations
    public void Read_ShouldThrow_WhenSchemeIsNotKnown(string testFile)
#pragma warning restore S4144 // Methods should not have identical implementations
    {
        var l3DXmlReader = new L3DXmlReader();

        var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        using (cache)
        {
            var act = () => l3DXmlReader.Read(cache);
            act.Should().Throw<InvalidL3DException>();
        }
    }

    private static IEnumerable<TestCaseData> GetInvalidTestFiles()
    {
        Setup.Initialize();
        foreach (var se in GetXmlFiles("invalid").Concat(Setup.InvalidVersionXmlFiles))
        {
            yield return new TestCaseData(se).SetArgDisplayNames(se.Replace(Setup.TestDataDirectory, "", StringComparison.Ordinal));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void Read_ShouldThrow_WhenXmlIsInvalid(string testFile)
    {
        var l3DXmlReader = new L3DXmlReader();

        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        // ReSharper disable once AccessToDisposedClosure
        var act = () => l3DXmlReader.Read(cache);
        act.Should().Throw<InvalidL3DException>();
    }

    private static IEnumerable<TestCaseData> GetValidTestFiles()
    {
        Setup.Initialize();
        foreach (var se in Setup.ExampleXmlFiles.Concat(Setup.ValidVersionXmlFiles))
        {
            yield return new TestCaseData(se).SetArgDisplayNames(se.Replace(Setup.TestDataDirectory, "", StringComparison.Ordinal));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetValidTestFiles))]
    public void Read_ShouldNotThrow_WhenXmlIsValid(string testFile)
    {
        var l3DXmlReader = new L3DXmlReader();

        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        // ReSharper disable once AccessToDisposedClosure
        var act = () => l3DXmlReader.Read(cache);
        act.Should().NotThrow();
    }
}