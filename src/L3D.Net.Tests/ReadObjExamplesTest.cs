using System.Collections.Generic;
using System.Linq;
using L3D.Net.Geometry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests
{
    [TestFixture]
    public class ReadObjExamplesTest
    {
        static List<string> ExampleFiles()
        {
            Setup.Initialize();
            return Setup.ExampleObjFiles.ToList();
        }

        [Test]
        [TestCaseSource(nameof(ExampleFiles))]
        public void ObjParser_ShouldBeAbleToReadAllExamples(string filename)
        {
            var parser = new ObjParser();
            parser.Parse(filename, Substitute.For<ILogger>());
        }
    }
}
