using L3D.Net.Internal.Abstract;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace L3D.Net.Internal;

public class FileHandler : IFileHandler
{
    public static readonly FileHandler Instance = new();

    public void CreateContainerFile(string directory, string containerPath)
    {
        ZipFile.CreateFromDirectory(directory, containerPath);
    }

    public byte[] CreateContainerByteArray(string directory)
    {
        var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        ZipFile.CreateFromDirectory(directory, tmpPath);

        var bytes = File.ReadAllBytes(tmpPath);
        if (File.Exists(tmpPath))
            File.Delete(tmpPath);

        return bytes;
    }

    public void AppendContainerToStream(string directory, Stream stream)
    {
        var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        ZipFile.CreateFromDirectory(directory, tmpPath);

        var bytes = File.ReadAllBytes(tmpPath);
        if (File.Exists(tmpPath))
            File.Delete(tmpPath);

        stream.Write(bytes, 0, bytes.Length);
    }

    public void ExtractContainerToDirectory(string containerPath, string directory)
    {
        ZipFile.ExtractToDirectory(containerPath, directory);
    }

    public void ExtractContainerToDirectory(byte[] containerBytes, string directory)
    {
        using var archiveMemoryStream = new MemoryStream(containerBytes);
        using var archive = new ZipArchive(archiveMemoryStream);
        archive.ExtractToDirectory(directory);
    }

    public void ExtractContainerToDirectory(Stream containerStream, string directory)
    {
        using var archive = new ZipArchive(containerStream);
        archive.ExtractToDirectory(directory);
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

    private static void CopyFile(string filepath, string targetDirectory)
    {
        var fileName = Path.GetFileName(filepath);
        var targetFileName = Path.Combine(targetDirectory, fileName);

        File.Copy(filepath, targetFileName);
    }

    private static void ThrowWhenModelIsInvalid(IModel3D model3D)
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