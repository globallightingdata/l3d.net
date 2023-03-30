using L3D.Net.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace L3D.Net.Tests;

[SetUpFixture]
public static class Setup
{
    private static bool _isInitialized;
    private static readonly object Lock = new();

    private static readonly MemoryStream Stream = new();

    public static string TestDataDirectory { get; private set; } = null!;
    public static string ExamplesDirectory { get; private set; } = null!;
    public static string ValidVersionsDirectory { get; private set; } = null!;
    public static string InvalidVersionsDirectory { get; private set; } = null!;

    public static IEnumerable<Stream> ExampleXmlStreams { get; private set; } = Array.Empty<Stream>();
    public static IEnumerable<string> ExampleXmlFiles { get; private set; } = Array.Empty<string>();
    public static IEnumerable<string> ExampleObjFiles { get; private set; } = Array.Empty<string>();
    public static IEnumerable<string> ValidVersionXmlFiles { get; private set; } = Array.Empty<string>();
    public static IEnumerable<string> InvalidVersionXmlFiles { get; private set; } = Array.Empty<string>();

    [OneTimeSetUp]
    public static void Initialize()
    {
        lock (Lock)
        {
            if (_isInitialized)
                return;

            var testBinDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            TestDataDirectory = Path.Combine(testBinDirectory, "TestData");

            ExamplesDirectory = Path.Combine(TestDataDirectory, "xml", "v0.9.2");
            ValidVersionsDirectory = Path.Combine(TestDataDirectory, "xml", "validation", "valid_versions");
            InvalidVersionsDirectory = Path.Combine(TestDataDirectory, "xml", "validation", "invalid_versions");

            ExampleXmlFiles = Directory.EnumerateFiles(ExamplesDirectory, "*.xml", SearchOption.AllDirectories).ToList();
            ExampleXmlStreams =
                ExampleXmlFiles.Select(x => File.Open(x, FileMode.Open, FileAccess.Read, FileShare.Read));
            ExampleObjFiles = Directory.EnumerateFiles(ExamplesDirectory, "*.obj", SearchOption.AllDirectories).ToList();
            ValidVersionXmlFiles = Directory.EnumerateFiles(ValidVersionsDirectory, "*.xml", SearchOption.AllDirectories).ToList();
            InvalidVersionXmlFiles = Directory.EnumerateFiles(InvalidVersionsDirectory, "*.xml", SearchOption.AllDirectories).ToList();

            _isInitialized = true;
        }
    }

    [OneTimeTearDown]
    public static void TearDown()
    {
        Stream.Dispose();
        foreach (var stream in ExampleXmlStreams)
        {
            stream.Dispose();
        }
    }

    public static List<string> EmptyStringValues()
    {
        return new List<string>
        {
            null!,
            "",
            " ",
            "\t",
            "\t ",
            "\t \t",
            Environment.NewLine
        };
    }

    public static List<byte[]> EmptyByteArrayValues()
    {
        return new List<byte[]>
        {
            null!,
            Array.Empty<byte>()
        };
    }

    public static List<Stream> EmptyStreamValues()
    {
        return new List<Stream>
        {
            null!,
            Stream
        };
    }

    public static readonly Dictionary<string, Func<Luminaire, Luminaire>> ExampleBuilderMapping =
        new()
        {
            {"example_000", luminaire => luminaire.BuildExample000()},
            {"example_001", luminaire => luminaire.BuildExample001()},
            {"example_002", luminaire => luminaire.BuildExample002()},
            {"example_003", luminaire => luminaire.BuildExample003()},
            {"example_004", luminaire => luminaire.BuildExample004()},
            {"example_005", luminaire => luminaire.BuildExample005()},
            {"example_006", luminaire => luminaire.BuildExample006()},
            {"example_007", luminaire => luminaire.BuildExample007()},
            {"example_008", luminaire => luminaire.BuildExample008()}
        };
}