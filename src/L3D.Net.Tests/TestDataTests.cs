using System;
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
    private static IEnumerable<TestCaseData> ExamplesConsistencyTestCases()
    {
        Setup.Initialize();
        foreach (var keyValuePair in Setup.ExampleBuilderMapping)
        {
            yield return new TestCaseData(new DirectoryInfo(Path.Combine(Setup.ExamplesDirectory, keyValuePair.Key)), keyValuePair.Value(new Luminaire()))
                .SetArgDisplayNames(keyValuePair.Key);
        }
    }

    [Test, TestCaseSource(nameof(ExamplesConsistencyTestCases))]
    public void ValidateExamples(DirectoryInfo exampleDirectory, Luminaire exampleBuild)
    {
        using var cache = exampleDirectory.ToCache();
        var luminaire = new L3DXmlReader().Read(cache);
        var expected = new Reader().ReadContainer(new Writer().WriteToByteArray(exampleBuild));
        luminaire.Should().BeEquivalentTo(expected, o => o.WithStrictOrdering().IncludingAllRuntimeProperties().AllowingInfiniteRecursion());
    }


    private static IEnumerable<TestCaseData> ExamplesValidationTestCases()
    {
        Setup.Initialize();
        foreach (var keyValuePair in Setup.ExampleBuilderMapping)
        {
            using var cache = new DirectoryInfo(Path.Combine(Setup.ExamplesDirectory, keyValuePair.Key)).ToCache();
            var luminaire = new L3DXmlReader().Read(cache)!;
            var written = new Writer().WriteToByteArray(luminaire);
            foreach (var validation in Enum.GetValues<Validation>())
            {
                yield return new TestCaseData(written, validation).SetArgDisplayNames(keyValuePair.Key, validation.ToString("G"));
            }
        }
    }

    [Test, TestCaseSource(nameof(ExamplesValidationTestCases))]
    public void ValidateExamples_ShouldHaveNoValidationHints(byte[] luminaire, Validation validation)
    {
        new Validator().ValidateContainer(luminaire, validation).Should().BeEmpty();
    }
}