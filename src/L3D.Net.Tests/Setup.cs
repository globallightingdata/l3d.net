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

#if NET9_0_OR_GREATER
    private static readonly System.Threading.Lock Lock = new();
#else
    private static readonly object Lock = new();
#endif

    private static readonly MemoryStream Stream = new();

    public static string TestDataDirectory { get; private set; } = null!;

    public static string ExamplesDirectory { get; private set; } = null!;

    public static string ValidationDirectory { get; private set; } = null!;

    public static string ValidVersionsDirectory { get; private set; } = null!;

    public static string InvalidVersionsDirectory { get; private set; } = null!;

    public static IEnumerable<Stream> ExampleXmlStreams { get; private set; } = [];

    public static IEnumerable<string> ExampleXmlFiles { get; private set; } = [];

    public static IEnumerable<string> ExampleObjFiles { get; private set; } = [];

    public static IEnumerable<string> ValidVersionXmlFiles { get; private set; } = [];

    public static IEnumerable<string> InvalidVersionXmlFiles { get; private set; } = [];

    [OneTimeSetUp]
    public static void Initialize()
    {
        lock (Lock)
        {
            if (_isInitialized)
                return;

            var testBinDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            TestDataDirectory = Path.Combine(testBinDirectory, "TestData");

            ExamplesDirectory = Path.Combine(TestDataDirectory, "xml", "v0.11.0");
            ValidationDirectory = Path.Combine(TestDataDirectory, "xml", "validation");
            ValidVersionsDirectory = Path.Combine(ValidationDirectory, "valid_versions");
            InvalidVersionsDirectory = Path.Combine(ValidationDirectory, "invalid_versions");

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

        foreach (var tmpStream in PathExtensions.Streams)
        {
            tmpStream.Dispose();
        }
    }

    public static List<string> EmptyStringValues() =>
    [
        null!,
        "",
        " ",
        "\t",
        "\t ",
        "\t \t",
        Environment.NewLine
    ];

    public static IEnumerable<TestCaseData> EmptyByteArrayValues()
    {
        yield return new TestCaseData(null).SetArgDisplayNames("<null>");
        yield return new TestCaseData(Array.Empty<byte>()).SetArgDisplayNames("<empty byte array>");
    }

    public static IEnumerable<TestCaseData> EmptyStreamValues()
    {
        yield return new TestCaseData(null).SetArgDisplayNames("<null>");
        yield return new TestCaseData(Stream).SetArgDisplayNames("<empty stream>");
    }

    public static Dictionary<string, Func<Luminaire, Luminaire>> ExampleBuilderMapping { get; } = new()
    {
        ["example_000"] = luminaire => luminaire.BuildExample000(),
        ["example_001"] = luminaire => luminaire.BuildExample001(),
        ["example_002"] = luminaire => luminaire.BuildExample002(),
        ["example_003"] = luminaire => luminaire.BuildExample003(),
        ["example_004"] = luminaire => luminaire.BuildExample004(),
        ["example_005"] = luminaire => luminaire.BuildExample005(),
        ["example_006"] = luminaire => luminaire.BuildExample006(),
        ["example_007"] = luminaire => luminaire.BuildExample007(),
        ["example_008"] = luminaire => luminaire.BuildExample008(),
        ["example_009"] = luminaire => luminaire.BuildExample009(),
        ["example_010"] = luminaire => luminaire.BuildExample010(),
        ["example_011"] = luminaire => luminaire.BuildExample011(),
        ["example_012"] = luminaire => luminaire.BuildExample012(),
        ["example_013"] = luminaire => luminaire.BuildExample013(),
    };
}