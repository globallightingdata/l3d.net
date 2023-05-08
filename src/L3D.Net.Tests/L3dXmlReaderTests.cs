using FluentAssertions;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests;

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

    public static IEnumerable<string> GetNoRootTestFiles() => GetXmlFiles("no_root");

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

    private static IEnumerable<string> GetNoLocationTestFiles() => GetXmlFiles("no_scheme_location");

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

    private static IEnumerable<string> GetUnknownSchemeTestFiles() => GetXmlFiles("unknown_scheme");

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

    private static IEnumerable<string> GetInvalidTestFiles()
    {
        Setup.Initialize();
        return GetXmlFiles("invalid").Concat(Setup.InvalidVersionXmlFiles);
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void Read_ShouldThrow_WhenXmlIsInvalid(string testFile)
    {
        var l3DXmlReader = new L3DXmlReader();

        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        var act = () => l3DXmlReader.Read(cache);
        act.Should().Throw<InvalidL3DException>();
    }

    private static IEnumerable<string> GetValidTestFiles()
    {
        Setup.Initialize();
        return Setup.ExampleXmlFiles.Concat(Setup.ValidVersionXmlFiles);
    }

    [Test]
    [TestCaseSource(nameof(GetValidTestFiles))]
    public void Read_ShouldNotThrow_WhenXmlIsValid(string testFile)
    {
        var l3DXmlReader = new L3DXmlReader();

        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        var act = () => l3DXmlReader.Read(cache);
        act.Should().NotThrow();
    }
}