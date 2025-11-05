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
    private L3DXmlReader _reader = null!;

    [SetUp]
    public void SetUp()
    {
        Setup.Initialize();
        _reader = new L3DXmlReader();
    }

    [Test]
    public void Read_ShouldThrow_WhenXmlStreamIsNull()
    {
        var act = () => _reader.Read(new ContainerCache());
        act.Should().Throw<ArgumentException>();
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
    public void Read_ShouldThrow_WhenXmlHasNoRoot(string testFile)
    {
        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        // ReSharper disable once AccessToDisposedClosure
        var act = () => _reader.Read(cache);
        act.Should().Throw<InvalidL3DException>();
    }

    private static IEnumerable<TestCaseData> GetNoLocationTestFiles() => GenerateXmlTestCases("no_scheme_location");

    [Test]
    [TestCaseSource(nameof(GetNoLocationTestFiles))]
#pragma warning disable S4144 // Methods should not have identical implementations
    public void Read_ShouldThrow_WhenSchemeLocationIsMissing(string testFile)
#pragma warning restore S4144 // Methods should not have identical implementations
    {
        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        // ReSharper disable once AccessToDisposedClosure
        var act = () => _reader.Read(cache);
        act.Should().Throw<InvalidL3DException>();
    }

    private static IEnumerable<TestCaseData> GetUnknownSchemeTestFiles() => GenerateXmlTestCases("unknown_scheme");

    [Test]
    [TestCaseSource(nameof(GetUnknownSchemeTestFiles))]
#pragma warning disable S4144 // Methods should not have identical implementations
    public void Read_ShouldThrow_WhenSchemeIsNotKnown(string testFile)
#pragma warning restore S4144 // Methods should not have identical implementations
    {
        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        // ReSharper disable once AccessToDisposedClosure
        var act = () => _reader.Read(cache);
        act.Should().Throw<InvalidL3DException>();
    }

    private static IEnumerable<TestCaseData> GetInvalidTestFiles() => GenerateXmlTestCases("invalid").Concat(GenerateXmlTestCases(Setup.InvalidVersionXmlFiles));

    [Test]
    [TestCaseSource(nameof(GetInvalidTestFiles))]
    public void Read_ShouldThrow_WhenXmlIsInvalid(string testFile)
    {
        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        // ReSharper disable once AccessToDisposedClosure
        var act = () => _reader.Read(cache);
        act.Should().Throw<InvalidL3DException>();
    }

    private static IEnumerable<TestCaseData> GetValidTestFiles() => GenerateXmlTestCases(Setup.ExampleXmlFiles.Concat(Setup.ValidVersionXmlFiles));

    [Test]
    [TestCaseSource(nameof(GetValidTestFiles))]
    public void Read_ShouldNotThrow_WhenXmlIsValid(string testFile)
    {
        using var cache = Path.GetDirectoryName(testFile)!.ToCache(Path.GetFileName(testFile));
        // ReSharper disable once AccessToDisposedClosure
        var act = () => _reader.Read(cache);
        act.Should().NotThrow();
    }
}