using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Mapper.V0_11_0;
using L3D.Net.XML.V0_10_0.Dto;
using NUnit.Framework;

namespace L3D.Net.Tests.Mapper.V0_10_0
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class Vector3DMapperTests : MapperTestBase
    {
        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                    new AxisRotationDto(),
                    new AxisRotation())
                .SetArgDisplayNames("<new()>", "<new()>");
            yield return new TestCaseData(
                    new AxisRotationDto { Min = 0.2 },
                    new AxisRotation { Min = 0.2 })
                .SetArgDisplayNames(nameof(AxisRotationDto.Min), nameof(AxisRotation.Min));
            yield return new TestCaseData(
                    new AxisRotationDto { Max = 0.2 },
                    new AxisRotation { Max = 0.2 })
                .SetArgDisplayNames(nameof(AxisRotationDto.Max), nameof(AxisRotation.Max));
            yield return new TestCaseData(
                    new AxisRotationDto { Step = 0.2 },
                    new AxisRotation { Step = 0.2 })
                .SetArgDisplayNames(nameof(AxisRotationDto.Step), nameof(AxisRotation.Step));
            yield return new TestCaseData(
                    new AxisRotationDto
                    {
                        Min = 0.1,
                        Max = 0.2,
                        Step = 0.3
                    },
                    new AxisRotation
                    {
                        Min = 0.1,
                        Max = 0.2,
                        Step = 0.3
                    })
                .SetArgDisplayNames("<filled>", "<filled>");
        }

        private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Union(TestCases());

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDataModel(AxisRotationDto element, AxisRotation expected)
        {
            AxisRotationMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDto(AxisRotationDto expected, AxisRotation element)
        {
            AxisRotationMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDataModel(AxisRotationDto element, AxisRotation expected)
        {
            AxisRotationMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDto(AxisRotationDto expected, AxisRotation element)
        {
            AxisRotationMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }
    }
}
