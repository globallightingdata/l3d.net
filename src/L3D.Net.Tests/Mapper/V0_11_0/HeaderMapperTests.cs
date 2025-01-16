using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Mapper.V0_11_0;
using L3D.Net.XML.V0_11_0.Dto;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.Tests.Mapper.V0_11_0;

[TestFixture, Parallelizable(ParallelScope.Fixtures)]
public class HeaderMapperTests : MapperTestBase
{
    private static IEnumerable<TestCaseData> TestCases()
    {
        var utcNow = DateTime.UtcNow;
        yield return new TestCaseData(
                new HeaderDto {FormatVersion = new FormatVersionDto()},
                new Header {FormatVersion = new FormatVersion()})
            .SetArgDisplayNames("<new()>", "<new()>");
        yield return new TestCaseData(
                new HeaderDto {CreatedWithApplication = "app", FormatVersion = new FormatVersionDto()},
                new Header {CreatedWithApplication = "app", FormatVersion = new FormatVersion()})
            .SetArgDisplayNames(nameof(HeaderDto.CreatedWithApplication), nameof(Header.CreatedWithApplication));
        yield return new TestCaseData(
                new HeaderDto {CreationTimeCode = utcNow, FormatVersion = new FormatVersionDto()},
                new Header {CreationTimeCode = utcNow, FormatVersion = new FormatVersion()})
            .SetArgDisplayNames(nameof(HeaderDto.CreationTimeCode), nameof(Header.CreationTimeCode));
        yield return new TestCaseData(
                new HeaderDto {Description = "desc", FormatVersion = new FormatVersionDto()},
                new Header {Description = "desc", FormatVersion = new FormatVersion()})
            .SetArgDisplayNames(nameof(HeaderDto.Description), nameof(Header.Description));
        yield return new TestCaseData(
                new HeaderDto {Name = "name", FormatVersion = new FormatVersionDto()},
                new Header {Name = "name", FormatVersion = new FormatVersion()})
            .SetArgDisplayNames(nameof(HeaderDto.Name), nameof(Header.Name));
        yield return new TestCaseData(
                new HeaderDto {FormatVersion = new FormatVersionDto {Major = 1, Minor = 2, PreRelease = 3, PreReleaseSpecified = true}},
                new Header {FormatVersion = new FormatVersion {Major = 1, Minor = 2, PreRelease = 3, PreReleaseSpecified = true}})
            .SetArgDisplayNames(nameof(HeaderDto.FormatVersion), nameof(Header.FormatVersion));
        yield return new TestCaseData(
                new HeaderDto
                {
                    Name = "name",
                    Description = "desc",
                    CreationTimeCode = utcNow,
                    CreatedWithApplication = "app",
                    FormatVersion = new FormatVersionDto {Major = 1, Minor = 2, PreRelease = 3, PreReleaseSpecified = true}
                },
                new Header
                {
                    Name = "name",
                    Description = "desc",
                    CreationTimeCode = utcNow,
                    CreatedWithApplication = "app",
                    FormatVersion = new FormatVersion {Major = 1, Minor = 2, PreRelease = 3, PreReleaseSpecified = true}
                })
            .SetArgDisplayNames("<filled>", "<filled>");
    }

    private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

    [Test, TestCaseSource(nameof(AllTestCases))]
    public void ConvertNullable_ShouldReturnCorrectDataModel(HeaderDto element, Header expected)
    {
        HeaderMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected, opt => opt
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(50)))
            .WhenTypeIs<DateTime>());
    }

    [Test, TestCaseSource(nameof(AllTestCases))]
    public void ConvertNullable_ShouldReturnCorrectDto(HeaderDto expected, Header element)
    {
        HeaderMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected, opt => opt
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(50)))
            .WhenTypeIs<DateTime>());
    }

    [Test, TestCaseSource(nameof(TestCases))]
    public void Convert_ShouldReturnCorrectDataModel(HeaderDto element, Header expected)
    {
        HeaderMapper.Instance.Convert(element).Should().BeEquivalentTo(expected, opt => opt
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(50)))
            .WhenTypeIs<DateTime>());
    }

    [Test, TestCaseSource(nameof(TestCases))]
    public void Convert_ShouldReturnCorrectDto(HeaderDto expected, Header element)
    {
        HeaderMapper.Instance.Convert(element).Should().BeEquivalentTo(expected, opt => opt
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(50)))
            .WhenTypeIs<DateTime>());
    }
}