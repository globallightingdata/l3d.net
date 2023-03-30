using FluentAssertions;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace L3D.Net.Tests;

[TestFixture]
public class FileHandlerTests
{
    private readonly List<string> _filesToDelete = new();
    private readonly List<string> _directoriesToDelete = new();

    [TearDown]
    public void Deinit()
    {
        foreach (var file in _filesToDelete)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch
            {
                // ignored
            }
        }

        foreach (var directory in _directoriesToDelete)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch
            {
                // ignored
            }
        }
    }

    private IModel3D CreateFakeModel3D()
    {
        var modelPath = Path.GetTempFileName();
        _filesToDelete.Add(modelPath);

        var model3D = Substitute.For<IModel3D>();
        model3D.FilePath.Returns(modelPath);
        return model3D;
    }

    public enum ContainerTypeToTest
    {
        Path,
        Bytes,
        Stream
    }

    public static IEnumerable<ContainerTypeToTest> ContainerTypeToTestEnumValues => Enum.GetValues<ContainerTypeToTest>();

    [Test]
    public void CreateContainerFile_ShouldZipGivenDirectoryToGivenPath()
    {
        var sourceDirectory = Path.Combine(Setup.ExamplesDirectory, "example_002");
        var sourceXml = Path.Combine(sourceDirectory, Constants.L3dXmlFilename);
        var geometryDirectories = Directory.GetDirectories(sourceDirectory).ToArray();
        var targetZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        var testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        _filesToDelete.Add(targetZipPath);
        _directoriesToDelete.Add(testDirectory);

        using var cache = new ContainerCache
        {
            StructureXml = File.OpenRead(sourceXml),
            Geometries = geometryDirectories.ToDictionary(x => Path.GetFileName(x), y =>
            {
                var geometries = new Dictionary<string, Stream>();

                var files = Directory.GetFiles(y);
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);

                    geometries.Add(fileName, File.OpenRead(file));
                }

                return geometries;
            })
        };

        new FileHandler().CreateContainerFile(cache, targetZipPath);

        File.Exists(targetZipPath).Should().BeTrue();

        ZipFile.ExtractToDirectory(targetZipPath, testDirectory);

        var sourceFiles = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories).Select(filePath => Path.GetRelativePath(sourceDirectory, filePath)).ToList();
        var unzippedFiles = Directory.EnumerateFiles(testDirectory, "*", SearchOption.AllDirectories).Select(filePath => Path.GetRelativePath(testDirectory, filePath)).ToList();

        unzippedFiles.Should().BeEquivalentTo(sourceFiles);
    }

    [Test]
    public void LoadModelFiles_ShouldThrowArgumentNullException_WhenModel3DIsNull()
    {
        var fileHandler = new FileHandler();

        var action = () => fileHandler.LoadModelFiles(null!, Guid.NewGuid().ToString(), new ContainerCache());

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void LoadModelFiles_ShouldThrowArgumentNullException_WhenModel3DHasNoAbsolutePath()
    {
        var fileHandler = new FileHandler();

        var action = () => fileHandler.LoadModelFiles(Substitute.For<IModel3D>(), Guid.NewGuid().ToString(), new ContainerCache());

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void LoadModelFiles_ShouldThrowArgumentException_WhenGeometryIdIsNullOrEmpty(string geometryId)
    {
        var fileHandler = new FileHandler();

        var action = () => fileHandler.LoadModelFiles(CreateFakeModel3D(), geometryId, new ContainerCache());

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void LoadModelFiles_ShouldThrowArgumentException_WhenModelHasNullOrEmptyLibraryPaths(string path)
    {
        var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _directoriesToDelete.Add(targetTestDirectory);
        var model3D = CreateFakeModel3D();

        model3D.ReferencedMaterialLibraryFiles.Returns(new[] { path });

        var fileHandler = new FileHandler();
        var action = () => fileHandler.LoadModelFiles(model3D, "someId", new ContainerCache());

        action.Should().Throw<ArgumentException>().WithMessage("The given model has null or empty material library paths");
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void LoadModelFiles_ShouldThrowArgumentException_WhenModelHasNullOrEmptyTexturePaths(string path)
    {
        var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _directoriesToDelete.Add(targetTestDirectory);
        var model3D = CreateFakeModel3D();

        model3D.ReferencedTextureFiles.Returns(new[] { path });

        var fileHandler = new FileHandler();
        var action = () => fileHandler.LoadModelFiles(model3D, "someId", new ContainerCache());

        action.Should().Throw<ArgumentException>().WithMessage("The given model has null or empty texture paths");
    }

    [Test]
    public void GetTextureBytes_ShouldThrowArgumentNullException_WhenDirectoryScopeIsNull()
    {
        var fileHandler = new FileHandler();
        var action = () =>
            fileHandler.GetTextureBytes(null!, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void GetTextureBytes_ShouldThrowArgumentException_WhenGeomIdIsNullOrEmpty(string geomId)
    {
        var fileHandler = new FileHandler();
        var action = () =>
            fileHandler.GetTextureBytes(new ContainerCache(), geomId, Guid.NewGuid().ToString());

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void GetTextureBytes_ShouldThrowArgumentException_WhenTextureNameIsNullOrEmpty(string textureName)
    {
        var fileHandler = new FileHandler();
        var action = () =>
            fileHandler.GetTextureBytes(new ContainerCache(), Guid.NewGuid().ToString(), textureName);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetTextureBytes_ShouldReturnFileBytes_WhenCorrect()
    {
        var examplePath = Path.Combine(Setup.TestDataDirectory, "xml", "v0.9.2", "example_008");
        var geomId = "cube";
        var textureName = "CubeTexture.png";
        var expectedPath = Path.Combine(examplePath, geomId, textureName);
        var expectedBytes = File.ReadAllBytes(expectedPath);

        using var cache = new ContainerCache();
        var memStream = new MemoryStream(expectedBytes);
        var files = new Dictionary<string, Stream>
        {
            { textureName, memStream }
        };

        cache.Geometries.Add(geomId, files);

        var fileHandler = new FileHandler();
        var textureBytes = fileHandler.GetTextureBytes(cache, geomId, textureName);

        textureBytes.Should().BeEquivalentTo(expectedBytes);
    }
}