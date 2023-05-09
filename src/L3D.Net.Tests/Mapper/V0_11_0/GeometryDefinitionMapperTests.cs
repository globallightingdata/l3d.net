using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Mapper.V0_11_0;
using L3D.Net.XML.V0_11_0.Dto;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.Tests.Mapper.V0_11_0;

[TestFixture, Parallelizable(ParallelScope.Fixtures)]
public class GeometryDefinitionMapperTests : MapperTestBase
{
    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
                new GeometryFileDefinitionDto(),
                new GeometryFileDefinition())
            .SetArgDisplayNames("<new()>", "<new()>");
        yield return new TestCaseData(
                new GeometryFileDefinitionDto { GeometryId = "id" },
                new GeometryFileDefinition { GeometryId = "id" })
            .SetArgDisplayNames(nameof(GeometryFileDefinitionDto.GeometryId), nameof(GeometryFileDefinition.GeometryId));
        yield return new TestCaseData(
                new GeometryFileDefinitionDto { FileName = "name" },
                new GeometryFileDefinition { FileName = "name" })
            .SetArgDisplayNames(nameof(GeometryFileDefinitionDto.FileName), nameof(GeometryFileDefinition.FileName));
        yield return new TestCaseData(
                new GeometryFileDefinitionDto { Units = GeometricUnitsDto.foot },
                new GeometryFileDefinition { Units = GeometricUnits.foot })
            .SetArgDisplayNames(nameof(GeometryFileDefinitionDto.Units), nameof(GeometryFileDefinition.Units));
        yield return new TestCaseData(
                new GeometryFileDefinitionDto
                {
                    GeometryId = "id",
                    FileName = "name",
                    Units = GeometricUnitsDto.foot
                },
                new GeometryFileDefinition
                {
                    GeometryId = "id",
                    FileName = "name",
                    Units = GeometricUnits.foot
                })
            .SetArgDisplayNames("<filled>", "<filled>");
    }

    private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

    [Test, TestCaseSource(nameof(AllTestCases))]
    public void ConvertNullable_ShouldReturnCorrectDataModel(GeometryFileDefinitionDto element, GeometryFileDefinition expected)
    {
        GeometryDefinitionMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
    }

    [Test, TestCaseSource(nameof(AllTestCases))]
    public void ConvertNullable_ShouldReturnCorrectDto(GeometryFileDefinitionDto expected, GeometryFileDefinition element)
    {
        GeometryDefinitionMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
    }

    [Test, TestCaseSource(nameof(TestCases))]
    public void Convert_ShouldReturnCorrectDataModel(GeometryFileDefinitionDto element, GeometryFileDefinition expected)
    {
        GeometryDefinitionMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
    }

    [Test, TestCaseSource(nameof(TestCases))]
    public void Convert_ShouldReturnCorrectDto(GeometryFileDefinitionDto expected, GeometryFileDefinition element)
    {
        GeometryDefinitionMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
    }
}