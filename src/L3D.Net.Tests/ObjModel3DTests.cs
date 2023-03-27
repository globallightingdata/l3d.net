using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Geometry;
using NUnit.Framework;
using System;

namespace L3D.Net.Tests;

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
                Normals = new(),
                FaceGroups = new(),
                Materials = new(),
                TextureCoordinates = new(),
                Vertices = new()
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
                FaceGroups = new()
                {
                    new()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Faces = new()
                    }
                },
                Materials = new(),
                Normals = new(),
                TextureCoordinates = new(),
                Vertices = new()
            }
        };

        var valid = model3D.IsFaceIndexValid(0, 0);

        valid.Should().BeFalse();
    }
}