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
    public class FaceAssignmentMapperTests : MapperTestBase
    {
        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                    new SingleFaceAssignmentDto(),
                    new SingleFaceAssignment())
                .SetArgDisplayNames("<new SingleFaceAssignmentDto()>", "<new SingleFaceAssignment()>");
            yield return new TestCaseData(
                    new FaceRangeAssignmentDto(),
                    new FaceRangeAssignment())
                .SetArgDisplayNames("<new FaceRangeAssignmentDto()>", "<new FaceRangeAssignment()>");
            yield return new TestCaseData(
                    new SingleFaceAssignmentDto { GroupIndex = 11 },
                    new SingleFaceAssignment { GroupIndex = 11 })
                .SetArgDisplayNames(nameof(SingleFaceAssignmentDto.GroupIndex), nameof(SingleFaceAssignment.GroupIndex));
            yield return new TestCaseData(
                    new SingleFaceAssignmentDto { FaceIndex = 11 },
                    new SingleFaceAssignment { FaceIndex = 11 })
                .SetArgDisplayNames(nameof(SingleFaceAssignmentDto.FaceIndex), nameof(SingleFaceAssignment.FaceIndex));
            yield return new TestCaseData(
                    new SingleFaceAssignmentDto
                    {
                        GroupIndex = 11,
                        FaceIndex = 12
                    },
                    new SingleFaceAssignment
                    {
                        GroupIndex = 11,
                        FaceIndex = 12
                    })
                .SetArgDisplayNames("<filled SingleFaceAssignmentDto>", "<filled SingleFaceAssignment>");
            yield return new TestCaseData(
                    new FaceRangeAssignmentDto { GroupIndex = 11 },
                    new FaceRangeAssignment { GroupIndex = 11 })
                .SetArgDisplayNames(nameof(FaceRangeAssignmentDto.GroupIndex), nameof(FaceRangeAssignment.GroupIndex));
            yield return new TestCaseData(
                    new FaceRangeAssignmentDto { FaceIndexBegin = 11 },
                    new FaceRangeAssignment { FaceIndexBegin = 11 })
                .SetArgDisplayNames(nameof(FaceRangeAssignmentDto.FaceIndexBegin), nameof(FaceRangeAssignment.FaceIndexBegin));
            yield return new TestCaseData(
                    new FaceRangeAssignmentDto { FaceIndexEnd = 11 },
                    new FaceRangeAssignment { FaceIndexEnd = 11 })
                .SetArgDisplayNames(nameof(FaceRangeAssignmentDto.FaceIndexEnd), nameof(FaceRangeAssignment.FaceIndexEnd));
            yield return new TestCaseData(
                    new FaceRangeAssignmentDto
                    {
                        GroupIndex = 11,
                        FaceIndexBegin = 12,
                        FaceIndexEnd = 13
                    },
                    new FaceRangeAssignment
                    {
                        GroupIndex = 11,
                        FaceIndexBegin = 12,
                        FaceIndexEnd = 13
                    })
                .SetArgDisplayNames("<filled FaceRangeAssignmentDto>", "<filled SingleFaceAssignment>");
        }

        private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDataModel(FaceAssignmentDto element, FaceAssignment expected)
        {
            FaceAssignmentMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDto(FaceAssignmentDto expected, FaceAssignment element)
        {
            FaceAssignmentMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDataModel(FaceAssignmentDto element, FaceAssignment expected)
        {
            FaceAssignmentMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDto(FaceAssignmentDto expected, FaceAssignment element)
        {
            FaceAssignmentMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }
    }
}
