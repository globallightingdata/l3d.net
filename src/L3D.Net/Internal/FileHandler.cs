using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using L3D.Net.Internal.Abstract;

namespace L3D.Net.Internal
{
    internal class FileHandler : IFileHandler
    {
        public void CreateContainerFromDirectory(string directory, string containerPath)
        {
            ZipFile.CreateFromDirectory(directory, containerPath);
        }

        public void ExtractContainerToDirectory(string containerPath, string directory)
        {
            ZipFile.ExtractToDirectory(containerPath, directory);
        }

        public byte[] GetTextureBytes(string directory, string geomId, string textureName)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(geomId))
                throw new ArgumentException(@$"'{nameof(geomId)}' cannot be null or whitespace.", nameof(geomId));
            if (string.IsNullOrWhiteSpace(textureName))
                throw new ArgumentException(@$"'{nameof(textureName)}' cannot be null or whitespace.", nameof(textureName));

            var texturePath = Path.Combine(directory, geomId, textureName);

            return File.ReadAllBytes(texturePath);
        }

        public IContainerDirectory CreateContainerDirectory()
        {
            return new ContainerDirectory();
        }

        public void CopyModelFiles(IModel3D model3D, string targetDirectory)
        {
            ThrowWhenModelIsInvalid(model3D);

            if (string.IsNullOrWhiteSpace(targetDirectory))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(targetDirectory));

            if (Directory.Exists(targetDirectory))
                throw new ArgumentException($"The given directory already exists: {targetDirectory}");

            Directory.CreateDirectory(targetDirectory);

            CopyFile(model3D.FilePath, targetDirectory);

            foreach (var materialLibraryFilename in model3D.ReferencedMaterialLibraryFiles)
            {
                CopyFile(materialLibraryFilename, targetDirectory);
            }

            foreach (var textureFilename in model3D.ReferencedTextureFiles)
            {
                CopyFile(textureFilename, targetDirectory);
            }
        }

        private void CopyFile(string filepath, string targetDirectory)
        {
            var fileName = Path.GetFileName(filepath);
            var targetFileName = Path.Combine(targetDirectory, fileName);

            File.Copy(filepath, targetFileName);
        }

        private void ThrowWhenModelIsInvalid(IModel3D model3D)
        {
            if (model3D == null) throw new ArgumentNullException(nameof(model3D));

            if (string.IsNullOrWhiteSpace(model3D.FilePath))
                throw new ArgumentException("The given model has no valid AbsolutePath");

            if (model3D.ReferencedMaterialLibraryFiles.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("The given model has null or empty material library paths");

            if (model3D.ReferencedTextureFiles.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("The given model has null or empty texture paths");
        }
    }
}