using FluentAssertions;
using L3D.Net.Geometry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Numerics;

namespace L3D.Net.Tests;

[TestFixture]
public class ObjParserTest
{
    [Test]
    public void Parse_ShouldParseTwoGroupsObjCorrectly()
    {
        var objPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_groups.obj");
        var mtlPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_groups.mtl");

        using var cache = Setup.TestDataDirectory.ToCache();

        var parser = new ObjParser();

        var fileName = Path.GetFileName(objPath);
        var model = parser.Parse(fileName, cache.Geometries["obj"], Substitute.For<ILogger>());

        model!.FileName.Should().Be(fileName);
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().Contain(d => d.Key == Path.GetFileName(mtlPath));
        model.ReferencedTextureFiles.Should().HaveCount(0);
        model.Data!.Vertices.Should().HaveCount(16);
        model.Data.Normals.Should().HaveCount(12);
        model.Data.TextureCoordinates.Should().HaveCount(28);
        model.Data.FaceGroups.Should().HaveCount(2);
        model.Data.FaceGroups[0].Faces.Should().HaveCount(6);
        model.Data.FaceGroups[0].Name.Should().Be("Cube");
        model.Data.FaceGroups[1].Faces.Should().HaveCount(6);
        model.Data.FaceGroups[1].Name.Should().Be("Cube.001");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                foreach (var faceVertex in modelFace.Vertices)
                {
                    faceVertex.VertexIndex.Should().BePositive();
                    faceVertex.VertexIndex.Should().BeLessOrEqualTo(model.Data.Vertices.Count);

                    faceVertex.NormalIndex.Should().BePositive();
                    faceVertex.NormalIndex.Should().BeLessOrEqualTo(model.Data.Normals.Count);

                    faceVertex.TextureCoordinateIndex.Should().BePositive();
                    faceVertex.TextureCoordinateIndex.Should().BeLessOrEqualTo(model.Data.TextureCoordinates.Count);
                }
            }
        }
    }

    [Test]
    public void Parse_ShouldParseMaterialCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_008");
        var objPath = Path.Combine(examplePath, "cube", "textured_cube.obj");
        var mtlPath = Path.Combine(examplePath, "cube", "textured_cube.mtl");
        var textureFile = Path.Combine(examplePath, "cube", "CubeTexture.png");

        using var cache = examplePath.ToCache();

        var parser = new ObjParser();

        var fileName = Path.GetFileName(objPath);
        var model = parser.Parse(fileName, cache.Geometries["cube"], Substitute.For<ILogger>());

        model!.FileName.Should().Be(fileName);
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().Contain(d => d.Key == Path.GetFileName(mtlPath));
        model.ReferencedTextureFiles.Should().HaveCount(1);
        model.ReferencedTextureFiles.Should().Contain(d => d.Key == Path.GetFileName(textureFile));
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data.Materials.Should().HaveCount(1);
        var modelMaterial = model.Data.Materials.First();
        modelMaterial.Color.Should().BeEquivalentTo(new Vector3(0.8f, 0.8f, 0.8f));
        modelMaterial.Name.Should().Be("CubeMaterial");
        modelMaterial.TextureName.Should().Be("CubeTexture.png");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }
}