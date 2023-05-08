using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Mapper.V0_11_0;
using L3D.Net.XML.V0_11_0.Dto;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.Tests.Mapper.V0_11_0;

[TestFixture, Parallelizable(ParallelScope.Fixtures)]
public class FormatVersionMapperTests : MapperTestBase
{
    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
                new FormatVersionDto(),
                new FormatVersion())
            .SetArgDisplayNames("<new()>", "<new()>");
        yield return new TestCaseData(
                new FormatVersionDto { Minor = 2 },
                new FormatVersion { Minor = 2 })
            .SetArgDisplayNames(nameof(FormatVersion.Minor), nameof(FormatVersion.Minor));
        yield return new TestCaseData(
                new FormatVersionDto { Major = 1 },
                new FormatVersion { Major = 1 })
            .SetArgDisplayNames(nameof(FormatVersion.Major), nameof(FormatVersion.Major));
        yield return new TestCaseData(
                new FormatVersionDto { PreRelease = 3, PreReleaseSpecified = true },
                new FormatVersion { PreRelease = 3, PreReleaseSpecified = true })
            .SetArgDisplayNames(nameof(FormatVersion.PreRelease), nameof(FormatVersion.PreRelease));
        yield return new TestCaseData(
                new FormatVersionDto { PreReleaseSpecified = true },
                new FormatVersion { PreReleaseSpecified = true })
            .SetArgDisplayNames(nameof(FormatVersion.PreReleaseSpecified), nameof(FormatVersion.PreReleaseSpecified));
        yield return new TestCaseData(
                new FormatVersionDto
                {
                    Minor = 1,
                    Major = 2,
                    PreRelease = 3,
                    PreReleaseSpecified = true
                },
                new FormatVersion
                {
                    Minor = 1,
                    Major = 2,
                    PreRelease = 3,
                    PreReleaseSpecified = true
                })
            .SetArgDisplayNames("<filled>", "<filled>");
    }

    private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

    [Test, TestCaseSource(nameof(AllTestCases))]
    public void ConvertNullable_ShouldReturnCorrectDataModel(FormatVersionDto element, FormatVersion expected)
    {
        FormatVersionMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
    }

    [Test, TestCaseSource(nameof(AllTestCases))]
    public void ConvertNullable_ShouldReturnCorrectDto(FormatVersionDto expected, FormatVersion element)
    {
        FormatVersionMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
    }

    [Test, TestCaseSource(nameof(TestCases))]
    public void Convert_ShouldReturnCorrectDataModel(FormatVersionDto element, FormatVersion expected)
    {
        FormatVersionMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
    }

    [Test, TestCaseSource(nameof(TestCases))]
    public void Convert_ShouldReturnCorrectDto(FormatVersionDto expected, FormatVersion element)
    {
        FormatVersionMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
    }
}