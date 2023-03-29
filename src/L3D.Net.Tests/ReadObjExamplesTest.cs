using FluentAssertions;
using L3D.Net.Geometry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.Tests;

[TestFixture]
public class ReadObjExamplesTest
{
    private static List<string> ExampleFiles()
    {
        Setup.Initialize();
        return Setup.ExampleObjFiles.ToList();
    }

    [Test]
    [TestCaseSource(nameof(ExampleFiles))]
    public void ObjParser_ShouldBeAbleToReadAllExamples(string filename)
    {
        var parser = new ObjParser();
        var act = () => parser.Parse(filename, Substitute.For<ILogger>());
        act.Should().NotThrow();
    }
}