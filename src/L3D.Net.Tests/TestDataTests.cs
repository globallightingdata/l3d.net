using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML;
using NUnit.Framework;

namespace L3D.Net.Tests;

[TestFixture]
public class TestDataTests
{
    private static IEnumerable<TestCaseData> ValidationTests()
    {
        Setup.Initialize();
        foreach (var keyValuePair in Setup.ExampleBuilderMapping)
        {
            yield return new TestCaseData(new DirectoryInfo(Path.Combine(Setup.ExamplesDirectory, keyValuePair.Key)), keyValuePair.Value(new Luminaire()))
                .SetArgDisplayNames(keyValuePair.Key);
        }
    }

    [Test, TestCaseSource(nameof(ValidationTests))]
    public void ValidateExamples(DirectoryInfo exampleDirectory, Luminaire exampleBuild)
    {
        using var cache = new ContainerCache();
        cache.StructureXml = File.OpenRead(Path.Combine(exampleDirectory.FullName, "structure.xml"));
        foreach (var geometryDirectory in exampleDirectory.GetDirectories())
        {
            if (!cache.Geometries.TryGetValue(geometryDirectory.Name, out var files))
            {
                files = new Dictionary<string, Stream>();
                cache.Geometries.Add(geometryDirectory.Name, files);
            }

            foreach (var fileInfo in geometryDirectory.GetFiles())
            {
                files.Add(fileInfo.Name, File.OpenRead(fileInfo.FullName));
            }
        }

        var luminaire = new L3DXmlReader().Read(cache);
        var expected = new Reader().ReadContainer(new Writer().WriteToByteArray(exampleBuild));
        luminaire.Should().BeEquivalentTo(expected, o => o.WithStrictOrdering().IncludingAllRuntimeProperties().AllowingInfiniteRecursion());
    }

    [Test, TestCaseSource(nameof(ValidationTests))]
    public void ValidateExamples_ShouldHaveNoValidationHints(DirectoryInfo exampleDirectory, Luminaire _)
    {
        using var cache = new ContainerCache();
        cache.StructureXml = File.OpenRead(Path.Combine(exampleDirectory.FullName, "structure.xml"));
        foreach (var geometryDirectory in exampleDirectory.GetDirectories())
        {
            if (!cache.Geometries.TryGetValue(geometryDirectory.Name, out var files))
            {
                files = new Dictionary<string, Stream>();
                cache.Geometries.Add(geometryDirectory.Name, files);
            }

            foreach (var fileInfo in geometryDirectory.GetFiles())
            {
                files.Add(fileInfo.Name, File.OpenRead(fileInfo.FullName));
            }
        }

        var luminaire = new L3DXmlReader().Read(cache)!;
        new Validator().ValidateContainer(new Writer().WriteToByteArray(luminaire), Validation.All).Should().BeEmpty();
    }
}