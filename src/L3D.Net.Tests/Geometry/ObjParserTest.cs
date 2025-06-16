using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using L3D.Net.Geometry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests.Geometry;

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
    public void Parse_ShouldParseTwoGroupsObjCorrectly_WhenEmptyGroupsExists()
    {
        var objPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_groups_with_empty.obj");
        var mtlPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_groups_with_empty.mtl");

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
    public void Parse_Example008_ShouldParseMaterialCorrectly()
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

    [Test]
    public void ParseFromFile_Example008_ShouldParseMaterialCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_008");
        var objPath = Path.Combine(examplePath, "cube", "textured_cube.obj");
        var mtlPath = Path.Combine(examplePath, "cube", "textured_cube.mtl");
        var textureFile = Path.Combine(examplePath, "cube", "CubeTexture.png");

        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());

        model.FileName.Should().BeEquivalentTo(Path.GetFileName(objPath));
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().ContainKey(Path.GetFileName(mtlPath));
        model.ReferencedTextureFiles.Should().HaveCount(1);
        model.ReferencedTextureFiles.First().Key.Should().Be(Path.GetFileName(textureFile));
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

    [Test]
    public void Parse_Example009_ShouldParseMaterialCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_009");
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
        modelMaterial.TextureName.Should().Be("./CubeTexture.png");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }

    [Test]
    public void ParseFromFile_Example009_ShouldParseMaterialCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_009");
        var objPath = Path.Combine(examplePath, "cube", "textured_cube.obj");
        var mtlPath = Path.Combine(examplePath, "cube", "textured_cube.mtl");
        var textureFile = Path.Combine(examplePath, "cube", "CubeTexture.png");

        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());

        model.FileName.Should().BeEquivalentTo(Path.GetFileName(objPath));
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().ContainKey(Path.GetFileName(mtlPath));
        model.ReferencedTextureFiles.Should().HaveCount(1);
        model.ReferencedTextureFiles.First().Key.Should().Be(Path.GetFileName(textureFile));
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data.Materials.Should().HaveCount(1);
        var modelMaterial = model.Data.Materials.First();
        modelMaterial.Color.Should().BeEquivalentTo(new Vector3(0.8f, 0.8f, 0.8f));
        modelMaterial.Name.Should().Be("CubeMaterial");
        modelMaterial.TextureName.Should().Be("./CubeTexture.png");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }

    [Test]
    public void Parse_Example010_ShouldParseMaterialCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_010");
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
        modelMaterial.TextureName.Should().Be("D:\\CubeTexture.png");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }

    [Test]
    public void ParseFromFile_Example010_ShouldParseMaterialCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_010");
        var objPath = Path.Combine(examplePath, "cube", "textured_cube.obj");
        var mtlPath = Path.Combine(examplePath, "cube", "textured_cube.mtl");
        var textureFile = Path.Combine(examplePath, "cube", "CubeTexture.png");

        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());

        model.FileName.Should().BeEquivalentTo(Path.GetFileName(objPath));
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().ContainKey(Path.GetFileName(mtlPath));
        model.ReferencedTextureFiles.Should().HaveCount(1);
        model.ReferencedTextureFiles.First().Key.Should().Be(Path.GetFileName(textureFile));
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data.Materials.Should().HaveCount(1);
        var modelMaterial = model.Data.Materials.First();
        modelMaterial.Color.Should().BeEquivalentTo(new Vector3(0.8f, 0.8f, 0.8f));
        modelMaterial.Name.Should().Be("CubeMaterial");
        modelMaterial.TextureName.Should().Be("D:\\CubeTexture.png");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }

    [Test]
    public void Parse_Example011_ShouldParseMaterialCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_011");
        var objPath = Path.Combine(examplePath, "cube", "textured_cube.obj");
        var mtlPath = Path.Combine(examplePath, "cube", "textured_cube");
        var textureFile = Path.Combine(examplePath, "cube", "CubeTexture");

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
        modelMaterial.TextureName.Should().Be("CubeTexture");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }

    [Test]
    public void ParseFromFile_Example011_ShouldParseMaterialCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_011");
        var objPath = Path.Combine(examplePath, "cube", "textured_cube.obj");
        var mtlPath = Path.Combine(examplePath, "cube", "textured_cube");
        var textureFile = Path.Combine(examplePath, "cube", "CubeTexture");

        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());

        model.FileName.Should().BeEquivalentTo(Path.GetFileName(objPath));
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().ContainKey(Path.GetFileName(mtlPath));
        model.ReferencedTextureFiles.Should().HaveCount(1);
        model.ReferencedTextureFiles.First().Key.Should().Be(Path.GetFileName(textureFile));
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data.Materials.Should().HaveCount(1);
        var modelMaterial = model.Data.Materials.First();
        modelMaterial.Color.Should().BeEquivalentTo(new Vector3(0.8f, 0.8f, 0.8f));
        modelMaterial.Name.Should().Be("CubeMaterial");
        modelMaterial.TextureName.Should().Be("CubeTexture");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }

    [Test]
    public void ParseFromFile_ShouldParseTwoGroupsObjCorrectly_WhenEmptyGroupsExists()
    {
        var objPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_groups_with_empty.obj");
        var mtlPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_groups_with_empty.mtl");

        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());

        model.FileName.Should().BeEquivalentTo(Path.GetFileName(objPath));
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().ContainKey(Path.GetFileName(mtlPath));
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
    public void ParseFromFile_ShouldParseTwoGroupsObjCorrectly()
    {
        var objPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_groups.obj");
        var mtlPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_groups.mtl");

        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());

        model.FileName.Should().BeEquivalentTo(Path.GetFileName(objPath));
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().ContainKey(Path.GetFileName(mtlPath));
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
    public void ParseFromFile_ShouldParseMaterialCorrectly_Example_008_IfMaterialsOrTexturesMissing_UsingPath()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_008");
        var objPath = Path.Combine(examplePath, "cube", "textured_cube.obj");
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.Copy(objPath, tmpFile);

        var parser = new ObjParser();

        var model = parser.Parse(tmpFile, Substitute.For<ILogger>());

        model.FileName.Should().BeEquivalentTo(Path.GetFileName(tmpFile));
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(0);
        model.ReferencedTextureFiles.Should().HaveCount(0);
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data.Materials.Should().HaveCount(0);

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(-1);
            }
        }

        File.Delete(tmpFile);
    }

    [Test]
    public void ParseFromFile_ShouldParseMaterialCorrectly_Example_008_IfMaterialsOrTexturesMissing_UsingCache()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_008");

        using var cache = examplePath.ToCache();

        var parser = new ObjParser();

        var model = parser.Parse("textured_cube.obj", new Dictionary<string, Stream> {["textured_cube.obj"] = cache.Geometries["cube"]["textured_cube.obj"]},
            Substitute.For<ILogger>());

        model!.FileName.Should().BeEquivalentTo("textured_cube.obj");
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(0);
        model.ReferencedTextureFiles.Should().HaveCount(0);
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data.Materials.Should().HaveCount(0);

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(-1);
            }
        }
    }

    [Test]
    public void ParseFromFile_ShouldParseMaterialCorrectly_Example_011_IfMaterialsOrTexturesMissing_UsingPath()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_011");
        var objPath = Path.Combine(examplePath, "cube", "textured_cube.obj");
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.Copy(objPath, tmpFile);

        var parser = new ObjParser();

        var model = parser.Parse(tmpFile, Substitute.For<ILogger>());

        model.FileName.Should().BeEquivalentTo(Path.GetFileName(tmpFile));
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(0);
        model.ReferencedTextureFiles.Should().HaveCount(0);
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data.Materials.Should().HaveCount(0);

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(-1);
            }
        }

        File.Delete(tmpFile);
    }

    [Test]
    public void ParseFromFile_ShouldParseMaterialCorrectly_Example_011_IfMaterialsOrTexturesMissing_UsingCache()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_011");

        using var cache = examplePath.ToCache();

        var parser = new ObjParser();

        var model = parser.Parse("textured_cube.obj", new Dictionary<string, Stream> {["textured_cube.obj"] = cache.Geometries["cube"]["textured_cube.obj"]},
            Substitute.For<ILogger>());

        model!.FileName.Should().BeEquivalentTo("textured_cube.obj");
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(0);
        model.ReferencedTextureFiles.Should().HaveCount(0);
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data.Materials.Should().HaveCount(0);

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(-1);
            }
        }
    }

    [Test]
    public void ParseFromFile_ShouldReturnNull_WhenObjNotFound()
    {
        var parser = new ObjParser();

        var model = parser.Parse("textured_cube.obj", new Dictionary<string, Stream>(), Substitute.For<ILogger>());

        model.Should().BeNull();
    }

    [Test]
    public void Parse_ShouldParseDefaultFaceGroupCorrectly()
    {
        var examplePath = Path.Combine(Setup.ExamplesDirectory, "example_000");
        var objPath = Path.Combine(examplePath, "cube", "cube.obj");
        var mtlPath = Path.Combine(examplePath, "cube", "cube.mtl");

        using var cache = examplePath.ToCache();

        var parser = new ObjParser();

        var fileName = Path.GetFileName(objPath);
        var model = parser.Parse(fileName, cache.Geometries["cube"], Substitute.For<ILogger>());

        model!.FileName.Should().Be(fileName);
        model.ReferencedMaterialLibraryFiles.Should().HaveCount(1);
        model.ReferencedMaterialLibraryFiles.Should().Contain(d => d.Key == Path.GetFileName(mtlPath));
        model.ReferencedTextureFiles.Should().HaveCount(0);
        model.Data!.FaceGroups.Should().HaveCount(1);
        model.Data!.FaceGroups[0].Name.Should().BeEquivalentTo("Default");
        model.Data.Materials.Should().HaveCount(1);
        var modelMaterial = model.Data.Materials.First();
        modelMaterial.Color.Should().BeEquivalentTo(new Vector3(0.8f, 0.8f, 0.8f));
        modelMaterial.Name.Should().Be("Material");

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }
}