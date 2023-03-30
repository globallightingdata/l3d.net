using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
// ReSharper disable ConvertToUsingDeclaration
#pragma warning disable IDE0063

namespace L3D.Net.Internal;

public class FileHandler : IFileHandler
{
    public static readonly FileHandler Instance = new();

    public void CreateContainerFile(ContainerCache cache, string containerPath)
    {
        using var memoryStream = CreateL3DStream(cache);

        using var fileStream = new FileStream(containerPath, FileMode.Create);
        memoryStream.Seek(0, SeekOrigin.Begin);
        memoryStream.CopyTo(fileStream);
    }

    public byte[] CreateContainerByteArray(ContainerCache cache)
    {
        using var memoryStream = CreateL3DStream(cache);

        return memoryStream.ToArray();
    }

    public void AppendContainerToStream(ContainerCache cache, Stream stream)
    {
        using var memoryStream = CreateL3DStream(cache);

        memoryStream.Seek(0, SeekOrigin.Begin);
        memoryStream.CopyTo(stream);
    }

    private static MemoryStream CreateL3DStream(ContainerCache cache)
    {
        if (cache.StructureXml == null)
            throw new ArgumentException($"{nameof(cache.StructureXml)} in {nameof(cache)} cannot be null");

        var memoryStream = new MemoryStream();
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
        var entry = archive.CreateEntry(Constants.L3dXmlFilename);

        cache.StructureXml.Seek(0, SeekOrigin.Begin);
        using (var entryStream = entry.Open())
        {
            cache.StructureXml.CopyTo(entryStream);
        }

        foreach (var geometry in cache.Geometries)
        {
            foreach (var file in geometry.Value)
            {
                entry = archive.CreateEntry(Path.Combine(geometry.Key, file.Key));

                file.Value.Seek(0, SeekOrigin.Begin);
                using (var geometryFileStream = entry.Open())
                {
                    file.Value.CopyTo(geometryFileStream);
                }
            }
        }

        return memoryStream;
    }

    private static ContainerCache LoadL3DStream(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);

        var entries = archive.Entries;

        if (!entries.Any(e => e.Name.Equals(Constants.L3dXmlFilename)))
            throw new InvalidL3DException("StructureXml could not be found");

        var cache = new ContainerCache();

        foreach (var entry in entries)
        {
            if (entry.Name.Equals(Constants.L3dXmlFilename, StringComparison.Ordinal))
            {
                using (var entryStream = entry.Open())
                {
                    cache.StructureXml = new MemoryStream();
                    entryStream.CopyTo(cache.StructureXml);
                }
            }
            else
            {
                var directoryName = Path.GetDirectoryName(entry.Name);
                if (string.IsNullOrWhiteSpace(directoryName)) continue;

                if (!cache.Geometries.TryGetValue(directoryName, out var files))
                {
                    files = new Dictionary<string, Stream>();
                    cache.Geometries.Add(directoryName, files);
                }

                using (var entryStream = entry.Open())
                {
                    var memStream = new MemoryStream();
                    entryStream.CopyTo(memStream);
                    files.Add(Path.GetFileName(entry.Name), memStream);
                }
            }
        }

        return cache;
    }

    public ContainerCache ExtractContainer(string containerPath)
    {
        using var fs = File.OpenRead(containerPath);
        return LoadL3DStream(fs);
    }

    public ContainerCache ExtractContainer(byte[] containerBytes)
    {
        using var archiveMemoryStream = new MemoryStream(containerBytes);
        return LoadL3DStream(archiveMemoryStream);
    }

    public ContainerCache ExtractContainer(Stream containerStream)
    {
        return LoadL3DStream(containerStream);
    }

    public byte[] GetTextureBytes(ContainerCache cache, string geomId, string textureName)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        if (string.IsNullOrWhiteSpace(geomId))
            throw new ArgumentException(@$"'{nameof(geomId)}' cannot be null or whitespace.", nameof(geomId));
        if (string.IsNullOrWhiteSpace(textureName))
            throw new ArgumentException(@$"'{nameof(textureName)}' cannot be null or whitespace.", nameof(textureName));

        if (!cache.Geometries.TryGetValue(geomId, out var files))
            return Array.Empty<byte>();

        if (!files.TryGetValue(textureName, out var stream))
            return Array.Empty<byte>();

        var buffer = new byte[stream.Length];
        _ = stream.Read(buffer, 0, buffer.Length);

        return buffer;
    }

    public void LoadModelFiles(IModel3D model3D, string geometryId, ContainerCache cache)
    {
        if (string.IsNullOrWhiteSpace(geometryId))
            throw new ArgumentException(@$"'{nameof(geometryId)}' cannot be null or whitespace.", nameof(geometryId));

        ThrowWhenModelIsInvalid(model3D);

        CopyFile(model3D.FilePath, geometryId, cache);

        foreach (var materialLibraryFilename in model3D.ReferencedMaterialLibraryFiles)
        {
            CopyFile(materialLibraryFilename, geometryId, cache);
        }

        foreach (var textureFilename in model3D.ReferencedTextureFiles)
        {
            CopyFile(textureFilename, geometryId, cache);
        }
    }

    private static void CopyFile(string filepath, string geometryId, ContainerCache cache)
    {
        var fileName = Path.GetFileName(filepath);

        if (!cache.Geometries.TryGetValue(geometryId, out var files))
        {
            files = new Dictionary<string, Stream>();
            cache.Geometries.Add(geometryId, files);
        }

        var stream = File.OpenRead(filepath);

        files.Add(fileName, stream);
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