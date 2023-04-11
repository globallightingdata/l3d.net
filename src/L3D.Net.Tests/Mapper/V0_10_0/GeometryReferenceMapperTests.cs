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
    public class GeometryReferenceMapperTests : MapperTestBase
    {
        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                    new GeometryReferenceDto(),
                    new GeometryFileDefinition())
                .SetArgDisplayNames("<new()>", "<new()>");
            yield return new TestCaseData(
                    new GeometryReferenceDto { GeometryId = "id" },
                    new GeometryFileDefinition { GeometryId = "id" })
                .SetArgDisplayNames(nameof(GeometryReferenceDto.GeometryId), nameof(GeometryFileDefinition.GeometryId));
            yield return new TestCaseData(
                    new GeometryReferenceDto
                    {
                        GeometryId = "id"
                    },
                    new GeometryFileDefinition
                    {
                        GeometryId = "id"
                    })
                .SetArgDisplayNames("<filled>", "<filled>");
        }

        private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDataModel(GeometryReferenceDto element, GeometryFileDefinition expected)
        {
            GeometryReferenceMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDto(GeometryReferenceDto expected, GeometryFileDefinition element)
        {
            GeometryReferenceMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDataModel(GeometryReferenceDto element, GeometryFileDefinition expected)
        {
            GeometryReferenceMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDto(GeometryReferenceDto expected, GeometryFileDefinition element)
        {
            GeometryReferenceMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }
    }
}
