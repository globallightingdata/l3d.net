using L3D.Net.XML.V0_11_0;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;

namespace L3D.Net.Tests;

[TestFixture]
public class ReadXmlExamplesTest
{
    private static IEnumerable<TestCaseData> ExampleFiles()
    {
        Setup.Initialize();
        var streams = Setup.ExampleXmlStreams.ToList();
        var files = Setup.ExampleXmlFiles.ToList();
        for (var i = 0; i < streams.Count; i++)
        {
            yield return new TestCaseData(streams[i])
                .SetArgDisplayNames(files[i].Replace(Setup.TestDataDirectory, "", StringComparison.Ordinal));
        }
    }

    [Test]
    [TestCaseSource(nameof(ExampleFiles))]
    public void Reader_ShouldBeAbleToReadAllExamples(Stream stream)
    {
        var serilizer = new XmlDtoSerializer();
        var luminaire = serilizer.Deserialize(stream);

        luminaire.Should().NotBeNull();
    }

    [Test]
    [Explicit("Performance")]
    public void Reader_Performance_Test()
    {
        Setup.Initialize();
        var examples = Setup.ExampleXmlStreams.ToList();

        var exampleCount = examples.Count;
        const int loopCount = 1000;

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var filesIndex = 0;
        for (var i = 0; i < loopCount; i++)
        {
            foreach (var stream in examples)
            {
                stopwatch.Stop();
                stream.Seek(0, SeekOrigin.Begin);
                stopwatch.Start();
                var serilizer = new XmlDtoSerializer();
                var luminaire = serilizer.Deserialize(stream);
                luminaire.Should().BeNull();

                if (filesIndex == 0)
                {
                    Console.WriteLine($"Elapsed time for the first file: {stopwatch.ElapsedMilliseconds}ms");
                }
                else if (filesIndex == 1)
                {
                    Console.WriteLine($"Elapsed time for the first two files: {stopwatch.ElapsedMilliseconds}ms");
                }
                else if (filesIndex == exampleCount - 1)
                {
                    Console.WriteLine($"Elapsed time for the first {exampleCount} files: {stopwatch.ElapsedMilliseconds}ms");
                }

                filesIndex++;
            }

            if (i == 0)
            {
                Console.WriteLine($"Elapsed time for the first loop: {stopwatch.ElapsedMilliseconds}ms");
            }
            else if (i == 1)
            {
                Console.WriteLine($"Elapsed time for the first two loops: {stopwatch.ElapsedMilliseconds}ms");
            }
            else if (i == 10)
            {
                Console.WriteLine($"Elapsed time for the first ten loops: {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        stopwatch.Stop();
        var totalCount = exampleCount * loopCount;
        Console.WriteLine(
            $"Performance: Total of {totalCount} xml files read in {stopwatch.ElapsedMilliseconds}ms with {(double) stopwatch.ElapsedMilliseconds / totalCount}ms per file");
    }
}