using FluentAssertions;
using L3D.Net.Geometry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
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
    public void ObjParser_ShouldBeAbleToReadAllExamples(string filePath)
    {
        var geometry = Path.GetDirectoryName(filePath)!;
        var l3d = Path.GetDirectoryName(geometry)!;
        var fileName = Path.GetFileName(filePath);
        var parser = new ObjParser();
        var act = () =>
        {
            using var cache = l3d.ToCache();
            return parser.Parse(fileName, cache.Geometries[Path.GetFileName(geometry)], Substitute.For<ILogger>());
        };
        act.Should().NotThrow();
    }
}