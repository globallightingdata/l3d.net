using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Geometry;
using NUnit.Framework;

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
            Data = new ModelData(
                Enumerable.Empty<Vector3>(),
                Enumerable.Empty<Vector3>(),
                Enumerable.Empty<Vector2>(),
                Enumerable.Empty<ModelFaceGroup>(),
                Enumerable.Empty<ModelMaterial>()
            )
        };

        var valid = model3D.IsFaceIndexValid(0, 0);

        valid.Should().BeFalse();
    }

    [Test]
    public void IsFaceIndexValid_ShouldReturnFalse_WhenModelHasFaceAtIndex()
    {
        var model3D = new ObjModel3D
        {
            Data = new ModelData(
                Enumerable.Empty<Vector3>(),
                Enumerable.Empty<Vector3>(),
                Enumerable.Empty<Vector2>(),
                new List<ModelFaceGroup> {new(Guid.NewGuid().ToString(), Enumerable.Empty<ModelFace>())},
                Enumerable.Empty<ModelMaterial>()
            )
        };

        var valid = model3D.IsFaceIndexValid(0, 0);

        valid.Should().BeFalse();
    }
}