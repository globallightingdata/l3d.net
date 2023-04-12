using L3D.Net.Abstract;
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

internal class FileHandler : IFileHandler
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
                entry = archive.CreateEntry($"{geometry.Key}/{file.Key}");

                file.Value.Seek(0, SeekOrigin.Begin);
                using (var geometryFileStream = entry.Open())
                {
                    file.Value.CopyTo(geometryFileStream);
                }
            }
        }

        return memoryStream;
    }

    private static ContainerCache? LoadL3DStream(Stream stream, bool canThrow)
    {
        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);

            var entries = archive.Entries;

            if (canThrow && !entries.Any(e => e.FullName.Equals(Constants.L3dXmlFilename)))
                throw new InvalidL3DException("StructureXml could not be found");

            var cache = new ContainerCache();

            foreach (var entry in entries)
            {
                if (entry.FullName.Equals(Constants.L3dXmlFilename, StringComparison.Ordinal))
                {
                    using (var entryStream = entry.Open())
                    {
                        cache.StructureXml = new MemoryStream();
                        entryStream.CopyTo(cache.StructureXml);
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(entry.Name)) continue;

                    var geometryName = entry.FullName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0];

                    if (!cache.Geometries.TryGetValue(geometryName, out var files))
                    {
                        files = new Dictionary<string, Stream>();
                        cache.Geometries.Add(geometryName, files);
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
        catch (InvalidDataException)
        {
            if (canThrow)
                throw;

            return null;
        }
    }

    public ContainerCache ExtractContainerOrThrow(string containerPath)
    {
        using var fs = File.OpenRead(containerPath);
        return LoadL3DStream(fs, true)!;
    }

    public ContainerCache ExtractContainerOrThrow(byte[] containerBytes)
    {
        using var archiveMemoryStream = new MemoryStream(containerBytes);
        return LoadL3DStream(archiveMemoryStream, true)!;
    }

    public ContainerCache ExtractContainerOrThrow(Stream containerStream)
    {
        return LoadL3DStream(containerStream, true)!;
    }

    public ContainerCache? ExtractContainer(string containerPath)
    {
        using var fs = File.OpenRead(containerPath);
        return LoadL3DStream(fs, false);
    }

    public ContainerCache? ExtractContainer(byte[] containerBytes)
    {
        using var archiveMemoryStream = new MemoryStream(containerBytes);
        return LoadL3DStream(archiveMemoryStream, false);
    }

    public ContainerCache? ExtractContainer(Stream containerStream)
    {
        return LoadL3DStream(containerStream, false);
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

        CopyFile(model3D.FileName, model3D.ObjFile, geometryId, cache);

        foreach (var materialLibraryFile in model3D.ReferencedMaterialLibraryFiles)
        {
            CopyFile(materialLibraryFile.Key, materialLibraryFile.Value, geometryId, cache);
        }

        foreach (var textureFile in model3D.ReferencedTextureFiles)
        {
            CopyFile(textureFile.Key, textureFile.Value, geometryId, cache);
        }
    }

    private static void CopyFile(string fileName, byte[] file, string geometryId, ContainerCache cache)
    {
        if (!cache.Geometries.TryGetValue(geometryId, out var files))
        {
            files = new Dictionary<string, Stream>();
            cache.Geometries.Add(geometryId, files);
        }

        var copy = new MemoryStream(file);
        files.Add(fileName, copy);
    }

    private static void ThrowWhenModelIsInvalid(IModel3D model3D)
    {
        if (model3D == null) throw new ArgumentNullException(nameof(model3D));

        if (string.IsNullOrWhiteSpace(model3D.FileName))
            throw new ArgumentException("The given model has no valid file name");

        if (model3D.ReferencedMaterialLibraryFiles.Any(x => string.IsNullOrWhiteSpace(x.Key)))
            throw new ArgumentException("The given model has null or empty material library paths");

        if (model3D.ReferencedTextureFiles.Any(x => string.IsNullOrWhiteSpace(x.Key)))
            throw new ArgumentException("The given model has null or empty texture paths");
    }
}