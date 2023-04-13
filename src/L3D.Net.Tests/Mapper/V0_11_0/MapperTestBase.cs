using System.Collections.Generic;
using NUnit.Framework;

namespace L3D.Net.Tests.Mapper.V0_11_0
{
    public abstract class MapperTestBase
    {
        protected static IEnumerable<TestCaseData> NullableTestCases()
        {
            yield return new TestCaseData(null, null).SetArgDisplayNames("<null>", "<null>");
        }
    }
}
