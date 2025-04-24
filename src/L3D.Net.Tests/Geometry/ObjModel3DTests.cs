using System;
using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Geometry;
using NUnit.Framework;

namespace L3D.Net.Tests.Geometry;

[TestFixture]
public class ObjModel3DTests
{
    [Test]
    public void IsFaceIndexValid_ShouldReturnFalse_WhenDataIsNotSet()
    {
        var model3D = new ObjModel3D();

        var valid = model3D.IsFaceIndexValid(0, 0);

        valid.Should().BeFalse();
    }

    [Test]
    public void IsFaceIndexValid_ShouldReturnFalse_WhenModelHasNoPartAtIndex()
    {
        var model3D = new ObjModel3D
        {
            Data = new ModelData
            {
                Normals = [],
                FaceGroups = [],
                Materials = [],
                TextureCoordinates = [],
                Vertices = []
            }
        };

        var valid = model3D.IsFaceIndexValid(0, 0);

        valid.Should().BeFalse();
    }

    [Test]
    public void IsFaceIndexValid_ShouldReturnFalse_WhenModelHasFaceAtIndex()
    {
        var model3D = new ObjModel3D
        {
            Data = new ModelData
            {
                FaceGroups =
                [
                    new()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Faces = []
                    }
                ],
                Materials = [],
                Normals = [],
                TextureCoordinates = [],
                Vertices = []
            }
        };

        var valid = model3D.IsFaceIndexValid(0, 0);

        valid.Should().BeFalse();
    }
}