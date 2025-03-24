using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Mapper.V0_11_0;
using L3D.Net.XML.V0_11_0.Dto;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.Tests.Mapper.V0_11_0;

[TestFixture, Parallelizable(ParallelScope.Fixtures)]
public class ShapeMapperTests : MapperTestBase
{
    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
                new RectangleDto(),
                new Rectangle())
            .SetArgDisplayNames("<new RectangleDto()>", "<new Rectangle()>");
        yield return new TestCaseData(
                new CircleDto(),
                new Circle())
            .SetArgDisplayNames("<new CircleDto()>", "<new Circle()>");
        yield return new TestCaseData(
                new RectangleDto {SizeX = 0.2},
                new Rectangle {SizeX = 0.2})
            .SetArgDisplayNames(nameof(RectangleDto.SizeX), nameof(Rectangle.SizeX));
        yield return new TestCaseData(
                new RectangleDto {SizeY = 0.2},
                new Rectangle {SizeY = 0.2})
            .SetArgDisplayNames(nameof(RectangleDto.SizeY), nameof(Rectangle.SizeY));
        yield return new TestCaseData(
                new CircleDto {Diameter = 0.2},
                new Circle {Diameter = 0.2})
            .SetArgDisplayNames(nameof(CircleDto.Diameter), nameof(Circle.Diameter));
        yield return new TestCaseData(
                new RectangleDto
                {
                    SizeX = 0.1,
                    SizeY = 0.2
                },
                new Rectangle
                {
                    SizeX = 0.1,
                    SizeY = 0.2
                })
            .SetArgDisplayNames("<filled RectangleDto>", "<filled Rectangle>");
    }

    private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

    [Test, TestCaseSource(nameof(AllTestCases))]
    public void ConvertNullable_ShouldReturnCorrectDataModel(ShapeDto element, Shape expected)
    {
        ShapeMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected, opt => opt.RespectingRuntimeTypes());
    }

    [Test, TestCaseSource(nameof(AllTestCases))]
    public void ConvertNullable_ShouldReturnCorrectDto(ShapeDto expected, Shape element)
    {
        ShapeMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected, opt => opt.RespectingRuntimeTypes());
    }

    [Test, TestCaseSource(nameof(TestCases))]
    public void Convert_ShouldReturnCorrectDataModel(ShapeDto element, Shape expected)
    {
        ShapeMapper.Instance.Convert(element).Should().BeEquivalentTo(expected, opt => opt.RespectingRuntimeTypes());
    }

    [Test, TestCaseSource(nameof(TestCases))]
    public void Convert_ShouldReturnCorrectDto(ShapeDto expected, Shape element)
    {
        ShapeMapper.Instance.Convert(element).Should().BeEquivalentTo(expected, opt => opt.RespectingRuntimeTypes());
    }
}