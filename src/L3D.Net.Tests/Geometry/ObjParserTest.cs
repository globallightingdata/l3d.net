using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Geometry;
using L3D.Net.Internal.Abstract;
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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "CubeMaterial",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            DiffuseTextureName = "CubeTexture.png",
            DiffuseTextureBytes = File.ReadAllBytes(textureFile),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "CubeMaterial",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            DiffuseTextureName = "CubeTexture.png",
            DiffuseTextureBytes = File.ReadAllBytes(textureFile),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "CubeMaterial",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            DiffuseTextureName = "./CubeTexture.png",
            DiffuseTextureBytes = File.ReadAllBytes(textureFile),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "CubeMaterial",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            DiffuseTextureName = "./CubeTexture.png",
            DiffuseTextureBytes = File.ReadAllBytes(textureFile),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "CubeMaterial",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            DiffuseTextureName = "D:\\CubeTexture.png",
            DiffuseTextureBytes = File.ReadAllBytes(textureFile),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "CubeMaterial",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            DiffuseTextureName = "D:\\CubeTexture.png",
            DiffuseTextureBytes = File.ReadAllBytes(textureFile),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "CubeMaterial",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            DiffuseTextureName = "CubeTexture",
            DiffuseTextureBytes = File.ReadAllBytes(textureFile),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "CubeMaterial",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            DiffuseTextureName = "CubeTexture",
            DiffuseTextureBytes = File.ReadAllBytes(textureFile),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

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
        model.Files.Values.Should().ContainSingle(e => e.Status == FileStatus.MissingMaterial);
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
        model.Files.Values.Should().ContainSingle(e => e.Status == FileStatus.MissingMaterial);
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
        model.Files.Values.Should().ContainSingle(e => e.Status == FileStatus.MissingMaterial);
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
        model.Files.Values.Should().ContainSingle(e => e.Status == FileStatus.MissingMaterial);
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
        model.Data.Materials[0].Should().BeEquivalentTo(new ModelMaterial
        {
            Name = "Material",
            DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
            SpecularExponent = 324f,
            AmbientColor = new Vector3(1, 1, 1),
            SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
            EmissiveColor = new Vector3(0, 0, 0),
            OpticalDensity = 1.45f,
            Dissolve = 1.0f,
            IlluminationModel = 2
        });

        foreach (var modelPart in model.Data.FaceGroups)
        {
            foreach (var modelFace in modelPart.Faces)
            {
                modelFace.MaterialIndex.Should().Be(0);
            }
        }
    }

    [Test]
    public void ParseFromFile_Sconce_ShouldParseMaterialCorrectly()
    {
        var objPath = Path.Combine(Setup.TestDataDirectory, "obj", "sconce_01.obj");
        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());
        model.Data!.Materials.Should().HaveCount(2);
        model.Data!.Materials.Should().BeEquivalentTo(new List<ModelMaterial>
        {
            new ModelMaterial
            {
                Name = "glass_milk_new",
                DiffuseColor = new Vector3(0.64f, 0.64f, 0.64f),
                SpecularExponent = 0,
                AmbientColor = new Vector3(1, 1, 1),
                SpecularColor = new Vector3(0, 0, 0),
                EmissiveColor = new Vector3(0, 0, 0),
                OpticalDensity = 1.5f,
                Dissolve = 1.0f,
                IlluminationModel = 2
            },
            new ModelMaterial
            {
                Name = "metal_grey_new",
                DiffuseColor = new Vector3(0.64f, 0.64f, 0.64f),
                SpecularExponent = 0,
                AmbientColor = new Vector3(1, 1, 1),
                SpecularColor = new Vector3(0, 0, 0),
                EmissiveColor = new Vector3(0, 0, 0),
                OpticalDensity = 1.5f,
                Dissolve = 1.0f,
                IlluminationModel = 2
            }
        }, o => o.WithStrictOrdering());
    }

    [Test]
    public void ParseFromFile_SingleMesh_ShouldParseMaterialCorrectly()
    {
        var objPath = Path.Combine(Setup.TestDataDirectory, "obj", "single_mesh.obj");
        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());
        model.Data!.Materials.Should().HaveCount(2);
        model.Data!.Materials.Should().BeEquivalentTo(new List<ModelMaterial>
        {
            new ModelMaterial
            {
                Name = "Material",
                DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
                SpecularExponent = 324,
                AmbientColor = new Vector3(1, 1, 1),
                SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
                EmissiveColor = new Vector3(0, 0, 0),
                OpticalDensity = 1.45f,
                Dissolve = 1.0f,
                IlluminationModel = 2
            },
            new ModelMaterial
            {
                Name = "None",
                DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
                SpecularExponent = 500,
                AmbientColor = new Vector3(0.8f, 0.8f, 0.8f),
                SpecularColor = new Vector3(0.8f, 0.8f, 0.8f),
                OpticalDensity = 1f,
                Dissolve = 1.0f,
                IlluminationModel = 2
            }
        }, o => o.WithStrictOrdering());
    }

    [Test]
    public void ParseFromFile_TwoObjects_ShouldParseMaterialCorrectly()
    {
        var objPath = Path.Combine(Setup.TestDataDirectory, "obj", "two_objects.obj");
        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());
        model.Data!.Materials.Should().HaveCount(2);
        model.Data!.Materials.Should().BeEquivalentTo(new List<ModelMaterial>
        {
            new ModelMaterial
            {
                Name = "Material",
                DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
                SpecularExponent = 324,
                AmbientColor = new Vector3(1, 1, 1),
                SpecularColor = new Vector3(0.5f, 0.5f, 0.5f),
                EmissiveColor = new Vector3(0, 0, 0),
                OpticalDensity = 1.45f,
                Dissolve = 1.0f,
                IlluminationModel = 2
            },
            new ModelMaterial
            {
                Name = "None",
                DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
                SpecularExponent = 500,
                AmbientColor = new Vector3(0.8f, 0.8f, 0.8f),
                SpecularColor = new Vector3(0.8f, 0.8f, 0.8f),
                OpticalDensity = 1f,
                Dissolve = 1.0f,
                IlluminationModel = 2
            }
        }, o => o.WithStrictOrdering());
    }

    [Test]
    public void ParseFromFile_FullMaterial_ShouldParseMaterialCorrectly()
    {
        var objPath = Path.Combine(Setup.TestDataDirectory, "obj", "full_material.obj");
        var parser = new ObjParser();

        var model = parser.Parse(objPath, Substitute.For<ILogger>());
        model.Data!.Materials.Should().HaveCount(1);
        model.Data!.Materials.Should().BeEquivalentTo(new List<ModelMaterial>
        {
            new ModelMaterial
            {
                Name = "Material",
                DiffuseColor = new Vector3(0.81f, 0.82f, 0.83f),
                DiffuseTextureName = "Kd",
                DiffuseTextureBytes = File.ReadAllBytes(Path.Combine(Setup.TestDataDirectory, "obj", "Kd")),
                SpecularExponent = 333.333f,
                AmbientColor = new Vector3(0.91f, 0.92f, 0.93f),
                AmbientTextureName = "Ka",
                AmbientTextureBytes = File.ReadAllBytes(Path.Combine(Setup.TestDataDirectory, "obj", "Ka")),
                SpecularColor = new Vector3(0.51f, 0.52f, 0.53f),
                SpecularTextureName = "Ks",
                SpecularTextureBytes = File.ReadAllBytes(Path.Combine(Setup.TestDataDirectory, "obj", "Ks")),
                EmissiveColor = new Vector3(0.21f, 0.22f, 0.23f),
                EmissiveTextureName = "Ke",
                EmissiveTextureBytes = File.ReadAllBytes(Path.Combine(Setup.TestDataDirectory, "obj", "Ke")),
                OpticalDensity = 1.45f,
                Dissolve = 0.9f,
                IlluminationModel = 3,
                Anisotropy = 1.5f,
                AnisotropyRotation = 1.4f,
                Metallic = 0.8f,
                MetallicTextureName = "Pm",
                MetallicTextureBytes = File.ReadAllBytes(Path.Combine(Setup.TestDataDirectory, "obj", "Pm")),
                Roughness = 0.7f,
                RoughnessTextureName = "Pr",
                RoughnessTextureBytes = File.ReadAllBytes(Path.Combine(Setup.TestDataDirectory, "obj", "Pr")),
                Sheen = 0.6f,
                SheenTextureName = "Ps",
                SheenTextureBytes = File.ReadAllBytes(Path.Combine(Setup.TestDataDirectory, "obj", "Ps")),
                ClearCoatThickness = 0.55f,
                ClearCoatRoughness = 0.56f,
                NormTextureName = "norm",
                NormTextureBytes = File.ReadAllBytes(Path.Combine(Setup.TestDataDirectory, "obj", "norm"))
            }
        }, o => o.WithStrictOrdering());
    }
}