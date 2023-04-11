using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Mapper.V0_11_0;
using L3D.Net.XML.V0_10_0.Dto;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.Tests.Mapper.V0_10_0
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class LuminousHeightsMapperTests : MapperTestBase
    {
        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                    new LuminousHeightsDto(),
                    new LuminousHeights())
                .SetArgDisplayNames("<new()>", "<new()>");
            yield return new TestCaseData(
                    new LuminousHeightsDto { C0 = 0.2 },
                    new LuminousHeights { C0 = 0.2 })
                .SetArgDisplayNames(nameof(LuminousHeightsDto.C0), nameof(LuminousHeights.C0));
            yield return new TestCaseData(
                    new LuminousHeightsDto { C90 = 0.2 },
                    new LuminousHeights { C90 = 0.2 })
                .SetArgDisplayNames(nameof(LuminousHeightsDto.C90), nameof(LuminousHeights.C90));
            yield return new TestCaseData(
                    new LuminousHeightsDto { C180 = 0.2 },
                    new LuminousHeights { C180 = 0.2 })
                .SetArgDisplayNames(nameof(LuminousHeightsDto.C180), nameof(LuminousHeights.C180));
            yield return new TestCaseData(
                    new LuminousHeightsDto { C270 = 0.2 },
                    new LuminousHeights { C270 = 0.2 })
                .SetArgDisplayNames(nameof(LuminousHeightsDto.C270), nameof(LuminousHeights.C270));
            yield return new TestCaseData(
                    new LuminousHeightsDto
                    {
                        C0 = 0.1,
                        C90 = 0.2,
                        C180 = 0.3,
                        C270 = 0.4
                    },
                    new LuminousHeights
                    {
                        C0 = 0.1,
                        C90 = 0.2,
                        C180 = 0.3,
                        C270 = 0.4
                    })
                .SetArgDisplayNames("<filled>", "<filled>");
        }

        private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDataModel(LuminousHeightsDto element, LuminousHeights expected)
        {
            LuminousHeightsMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDto(LuminousHeightsDto expected, LuminousHeights element)
        {
            LuminousHeightsMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDataModel(LuminousHeightsDto element, LuminousHeights expected)
        {
            LuminousHeightsMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDto(LuminousHeightsDto expected, LuminousHeights element)
        {
            LuminousHeightsMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }
    }
}
