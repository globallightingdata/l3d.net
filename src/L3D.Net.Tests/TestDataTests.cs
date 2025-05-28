using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.XML;
using NUnit.Framework;

namespace L3D.Net.Tests;

[TestFixture]
public class TestDataTests
{
    private static IEnumerable<TestCaseData> ExamplesTestCases()
    {
        Setup.Initialize();
        foreach (var keyValuePair in Setup.ExampleBuilderMapping)
        {
            yield return new TestCaseData(new DirectoryInfo(Path.Combine(Setup.ExamplesDirectory, keyValuePair.Key)), keyValuePair.Value(new Luminaire()))
                .SetArgDisplayNames(keyValuePair.Key);
        }
    }

    [Test, TestCaseSource(nameof(ExamplesTestCases))]
    public void ValidateExamples(DirectoryInfo exampleDirectory, Luminaire exampleBuild)
    {
        using var cache = exampleDirectory.ToCache();
        var luminaire = new L3DXmlReader().Read(cache);
        var expected = new Reader().ReadContainer(new Writer().WriteToByteArray(exampleBuild));
        luminaire.Should().BeEquivalentTo(expected, o => o.WithStrictOrdering().IncludingAllRuntimeProperties().AllowingInfiniteRecursion());
    }

    [Test, TestCaseSource(nameof(ExamplesTestCases))]
    public void ValidateExamples_ShouldHaveNoValidationHints(DirectoryInfo exampleDirectory, Luminaire _)
    {
        using var cache = exampleDirectory.ToCache();

        var luminaire = new L3DXmlReader().Read(cache)!;
        new Validator().ValidateContainer(new Writer().WriteToByteArray(luminaire), Validation.All).Should().BeEmpty();
    }
}