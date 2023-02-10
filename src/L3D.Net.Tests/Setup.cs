using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace L3D.Net.Tests
{
    [SetUpFixture]
    public class Setup
    {
        private static bool _isInitialized;
        private static readonly Mutex Mutex = new Mutex();
        
        public static string TestDataDirectory { get; private set; }
        public static string ExamplesDirectory { get; private set; }


        public static IEnumerable<string> ExampleXmlFiles { get; private set; }
        public static IEnumerable<string> ExampleObjFiles { get; private set; }

        [OneTimeSetUp]
        public static void Initialize()
        {
            lock (Mutex)
            {
                if (_isInitialized)
                    return;

                var testBinDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                TestDataDirectory = Path.Combine(testBinDirectory, "TestData");

                ExamplesDirectory = Path.Combine(TestDataDirectory, "xml", "v0.9.2");
                ExampleXmlFiles = Directory.EnumerateFiles(ExamplesDirectory, "*.xml", SearchOption.AllDirectories).ToList();
                ExampleObjFiles = Directory.EnumerateFiles(ExamplesDirectory, "*.obj", SearchOption.AllDirectories).ToList();

                _isInitialized = true;
            }
        }
        
        public static List<string> EmptyStringValues()
        {
            return new List<string>
            {
                null,
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
                null,
                Array.Empty<byte>()
            };
        }

        public static readonly Dictionary<string, Func<LuminaireBuilder, LuminaireBuilder>> ExampleBuilderMapping =
            new Dictionary<string, Func<LuminaireBuilder, LuminaireBuilder>>
            {
                {"example_000", builder => builder.BuildExample000()},
                {"example_001", builder => builder.BuildExample001()},
                {"example_002", builder => builder.BuildExample002()},
                {"example_003", builder => builder.BuildExample003()},
                {"example_004", builder => builder.BuildExample004()},
                {"example_005", builder => builder.BuildExample005()},
                {"example_006", builder => builder.BuildExample006()},
                {"example_007", builder => builder.BuildExample007()},
                {"example_008", builder => builder.BuildExample008()},
            };
    }
}
