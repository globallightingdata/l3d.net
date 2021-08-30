using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using L3D.Net.XML.V0_9_0;
using NUnit.Framework;

namespace L3D.Net.Tests
{
    [TestFixture]
    public class ReadXmlExamplesTest
    {
        static List<string> ExampleFiles()
        {
            Setup.Initialize();
            var exampleFiles = Setup.ExampleXmlFiles.ToList();
            return exampleFiles;
        }

        [Test]
        [TestCaseSource(nameof(ExampleFiles))]
        public void Reader_ShouldBeAbleToReadAllExamples(string filename)
        {
            var serilizer = new XmlDtoSerializer();
            var luminaire = serilizer.Deserialize(filename);

            Assert.NotNull(luminaire);
        }

        [Test]
        [Explicit("Performance")]
        public void Reader_Performance_Test()
        {
            var examples = ExampleFiles();

            int exampleCount = examples.Count;
            int loopCount = 1000;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int filesIndex = 0;
            for (int i = 0; i < loopCount; i++)
            {
                foreach (var filename in examples)
                {
                    var serilizer = new XmlDtoSerializer();
                    var luminaire = serilizer.Deserialize(filename);
                    Assert.NotNull(luminaire);

                    if (filesIndex == 0)
                    {
                        System.Console.WriteLine($"Elapsed time for the first file: {stopwatch.ElapsedMilliseconds}ms");
                    }
                    else if (filesIndex == 1)
                    {
                        System.Console.WriteLine($"Elapsed time for the first two files: {stopwatch.ElapsedMilliseconds}ms");
                    }
                    else if (filesIndex == exampleCount - 1)
                    {
                        System.Console.WriteLine($"Elapsed time for the first {exampleCount} files: {stopwatch.ElapsedMilliseconds}ms");
                    }

                    filesIndex++;
                }

                if (i == 0)
                {
                    System.Console.WriteLine($"Elapsed time for the first loop: {stopwatch.ElapsedMilliseconds}ms");
                }
                else if (i == 1)
                {
                    System.Console.WriteLine($"Elapsed time for the first two loops: {stopwatch.ElapsedMilliseconds}ms");
                }
                else if (i == 10)
                {
                    System.Console.WriteLine($"Elapsed time for the first ten loops: {stopwatch.ElapsedMilliseconds}ms");
                }
            }

            stopwatch.Stop();
            int totalCount = exampleCount * loopCount;
            System.Console.WriteLine($"Performance: Total of {totalCount} xml files read in {stopwatch.ElapsedMilliseconds}ms with {(double)stopwatch.ElapsedMilliseconds / totalCount}ms per file");
        }
    }
}