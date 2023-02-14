using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FluentAssertions;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests
{
    [TestFixture]
    public class FileHandlerTests
    {
        private readonly List<string> _filesToDelete = new List<string>();
        private readonly List<string> _directoriesToDelete = new List<string>();

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

        private IModel3D CreateFakeModel3DWithMaterialLibFiles()
        {
            var model3D = CreateFakeModel3D();
            var materialFiles = new List<string>();

            for (var i = 0; i < 3; i++)
            {
                var materialFile = Path.GetTempFileName();
                _filesToDelete.Add(materialFile);
                materialFiles.Add(materialFile);
            }

            model3D.ReferencedMaterialLibraryFiles.Returns(materialFiles);

            return model3D;
        }

        private IModel3D CreateFakeModel3DWithTextureFiles()
        {
            var model3D = CreateFakeModel3D();
            var textureFiles = new List<string>();

            for (var i = 0; i < 3; i++)
            {
                var materialFile = Path.GetTempFileName();
                _filesToDelete.Add(materialFile);
                textureFiles.Add(materialFile);
            }

            model3D.ReferencedTextureFiles.Returns(textureFiles);

            return model3D;
        }

        public enum ContainerTypeToTest
        {
            Path,
            Bytes
        }

        public static IEnumerable<ContainerTypeToTest> ContainerTypeToTestEnumValues => Enum.GetValues<ContainerTypeToTest>();

        [Test]
        public void CreateContianerFromDirectory_ShouldZipGivenDirectoryToGivenPath()
        {
            var sourceDirectory = Path.Combine(Setup.ExamplesDirectory, "example_002");
            var targetZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
            var testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            _filesToDelete.Add(targetZipPath);
            _directoriesToDelete.Add(testDirectory);

            new FileHandler().CreateContainerFromDirectory(sourceDirectory, targetZipPath);

            File.Exists(targetZipPath).Should().BeTrue();

            ZipFile.ExtractToDirectory(targetZipPath, testDirectory);

            var sourceFiles = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories).Select(filePath => Path.GetRelativePath(sourceDirectory, filePath)).ToList();
            var unzippedFiles = Directory.EnumerateFiles(testDirectory, "*", SearchOption.AllDirectories).Select(filePath => Path.GetRelativePath(testDirectory, filePath)).ToList();

            unzippedFiles.Should().BeEquivalentTo(sourceFiles);
        }

        [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
        public void ExtractContainerToDirectory_ShouldUnzipGivenPathToGivenDirectory(ContainerTypeToTest containerTypeToTest)
        {
            var sourceDirectory = Path.Combine(Setup.ExamplesDirectory, "example_002");
            var targetZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
            var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            _filesToDelete.Add(targetZipPath);
            _directoriesToDelete.Add(targetTestDirectory);

            ZipFile.CreateFromDirectory(sourceDirectory, targetZipPath);

            File.Exists(targetZipPath).Should().BeTrue();

            switch (containerTypeToTest)
            {
                case ContainerTypeToTest.Path:
                    new FileHandler().ExtractContainerToDirectory(targetZipPath, targetTestDirectory);
                    break;
                case ContainerTypeToTest.Bytes:
                    var containerBytes = File.ReadAllBytes(targetZipPath);
                    new FileHandler().ExtractContainerToDirectory(containerBytes, targetTestDirectory);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
            }

            var sourceFiles = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories).Select(filePath => Path.GetRelativePath(sourceDirectory, filePath)).ToList();
            var unzippedFiles = Directory.EnumerateFiles(targetTestDirectory, "*", SearchOption.AllDirectories).Select(filePath => Path.GetRelativePath(targetTestDirectory, filePath)).ToList();

            unzippedFiles.Should().BeEquivalentTo(sourceFiles);
        }

        [Test]
        public void CreateContainerDirectory_ShouldCreateNewInstanceWithNewDirectory()
        {
            var scope0 = new FileHandler().CreateContainerDirectory();
            var scope1 = new FileHandler().CreateContainerDirectory();
            var scope2 = new FileHandler().CreateContainerDirectory();

            using (new ContainerDirectoryScope(scope0))
            using (new ContainerDirectoryScope(scope1))
            using (new ContainerDirectoryScope(scope2))
            {
                scope0.Should().NotBeSameAs(scope1);
                scope1.Should().NotBeSameAs(scope2);

                scope0.Path.Should().NotBeEquivalentTo(scope1.Path);
                scope1.Path.Should().NotBeEquivalentTo(scope2.Path);
            }
        }

        [Test]
        public void CopyModelFiles_ShouldThrowArgumentNullException_WhenModel3DIsNull()
        {
            var fileHandler = new FileHandler();
            
            var action = () => fileHandler.CopyModelFiles(null, Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CopyModelFiles_ShouldThrowArgumentNullException_WhenModel3DHasNoAbsolutePath()
        {
            var fileHandler = new FileHandler();
            
            var action = () => fileHandler.CopyModelFiles(Substitute.For<IModel3D>(), Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void CopyModelFiles_ShouldThrowArgumentException_WhenTargetPathIsNullOrEmpty(string targetPath)
        {
            var fileHandler = new FileHandler();
            
            var action = () => fileHandler.CopyModelFiles(CreateFakeModel3D(), targetPath);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void CopyModelFiles_ShouldCreateTargetDirectory_WhenDirectoryIsNotAvailable()
        {
            var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _directoriesToDelete.Add(targetTestDirectory);
            var fileHandler = new FileHandler();

            var model3D = CreateFakeModel3D();

            fileHandler.CopyModelFiles(model3D, targetTestDirectory);

            Directory.Exists(targetTestDirectory).Should().BeTrue();
        }

        [Test]
        public void CopyModelFiles_ShouldThrowArgumentException_WhenDirectoryAlreadyExists()
        {
            var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _directoriesToDelete.Add(targetTestDirectory);

            Directory.CreateDirectory(targetTestDirectory);

            var fileHandler = new FileHandler();

            var action = () => fileHandler.CopyModelFiles(CreateFakeModel3D(), targetTestDirectory);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void CopyModelFiles_ShouldCopyModelFileToTargetDirectory()
        {
            var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _directoriesToDelete.Add(targetTestDirectory);
            var model3D = CreateFakeModel3D();
            var expectedFile = Path.Combine(targetTestDirectory, Path.GetFileName(model3D.FilePath)!);

            var fileHandler = new FileHandler();

            fileHandler.CopyModelFiles(model3D, targetTestDirectory);

            File.Exists(expectedFile).Should().BeTrue();
        }

        [Test]
        public void CopyModelFiles_ShouldCopyMaterialLibrariesToTargetDirectory()
        {
            var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _directoriesToDelete.Add(targetTestDirectory);
            var model3D = CreateFakeModel3DWithMaterialLibFiles();

            var fileHandler = new FileHandler();

            fileHandler.CopyModelFiles(model3D, targetTestDirectory);

            model3D.ReferencedMaterialLibraryFiles.Count().Should().BePositive();
            foreach (var materialLibrary in model3D.ReferencedMaterialLibraryFiles)
            {
                var expectedFile = Path.Combine(targetTestDirectory, Path.GetFileName(materialLibrary));
                File.Exists(expectedFile).Should().BeTrue();
            }
        }

        [Test]
        public void CopyModelFiles_ShouldCopyTextureFilesToTargetDirectory()
        {
            var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _directoriesToDelete.Add(targetTestDirectory);
            var model3D = CreateFakeModel3DWithTextureFiles();

            var fileHandler = new FileHandler();

            fileHandler.CopyModelFiles(model3D, targetTestDirectory);

            model3D.ReferencedTextureFiles.Count().Should().BePositive();
            foreach (var textureFile in model3D.ReferencedTextureFiles)
            {
                var expectedFile = Path.Combine(targetTestDirectory, Path.GetFileName(textureFile));
                File.Exists(expectedFile).Should().BeTrue();
            }
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void CopyModelFiles_ShouldThrowArgumentException_WhenModelHasNullOrEmptyLibraryPaths(string path)
        {
            var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _directoriesToDelete.Add(targetTestDirectory);
            var model3D = CreateFakeModel3D();
            
            model3D.ReferencedMaterialLibraryFiles.Returns(new []{path});

            var fileHandler = new FileHandler();
            var action = () => fileHandler.CopyModelFiles(model3D, targetTestDirectory);

            action.Should().Throw<ArgumentException>().WithMessage("The given model has null or empty material library paths");
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void CopyModelFiles_ShouldThrowArgumentException_WhenModelHasNullOrEmptyTexturePaths(string path)
        {
            var targetTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _directoriesToDelete.Add(targetTestDirectory);
            var model3D = CreateFakeModel3D();
            
            model3D.ReferencedTextureFiles.Returns(new []{path});

            var fileHandler = new FileHandler();
            var action = () => fileHandler.CopyModelFiles(model3D, targetTestDirectory);

            action.Should().Throw<ArgumentException>().WithMessage("The given model has null or empty texture paths");
        }

        [Test]
        public void GetTextureBytes_ShouldThrowArgumentNullException_WhenDirectoryScopeIsNull()
        {
            var fileHandler = new FileHandler();
            Action action = () =>
                fileHandler.GetTextureBytes(null, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void GetTextureBytes_ShouldThrowArgumentException_WhenGeomIdIsNullOrEmpty(string geomId)
        {
            var fileHandler = new FileHandler();
            Action action = () =>
                fileHandler.GetTextureBytes(Guid.NewGuid().ToString(), geomId, Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void GetTextureBytes_ShouldThrowArgumentException_WhenTextureNameIsNullOrEmpty(string textureName)
        {
            var fileHandler = new FileHandler();
            Action action = () =>
                fileHandler.GetTextureBytes(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), textureName);

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

            var fileHandler = new FileHandler();
            var textureBytes = fileHandler.GetTextureBytes(examplePath, geomId, textureName);

            textureBytes.Should().BeEquivalentTo(expectedBytes);
        }
    }
}
