using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Mapper.V0_11_0;
using L3D.Net.XML.V0_11_0.Dto;
using NUnit.Framework;

namespace L3D.Net.Tests.Mapper.V0_11_0
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class GeometricUnitsMapperTests : MapperTestBase
    {
        [Test, Sequential]
        public void Convert_ShouldReturnCorrectDataModel([Values] GeometricUnitsDto element, [Values] GeometricUnits expected)
        {
            GeometricUnitsMapper.Instance.Convert(element).Should().Be(expected);
        }

        [Test, Sequential]
        public void Convert_ShouldReturnCorrectDto([Values] GeometricUnitsDto expected, [Values] GeometricUnits element)
        {
            GeometricUnitsMapper.Instance.Convert(element).Should().Be(expected);
        }
    }
}
