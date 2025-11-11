using JeremyAnsel.Media.WavefrontObj;
using L3D.Net.Data;
using L3D.Net.Extensions;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using L3D.Net.Internal;
using System.Diagnostics.CodeAnalysis;

namespace L3D.Net.Geometry;

public class ObjParser : IObjParser
{
    public static readonly ObjParser Instance = new();

    private static readonly ObjFileReaderSettings ObjFileReaderSettings = new()
        {HandleObjectNamesAsGroup = true, OnlyOneGroupNamePerLine = true, KeepWhitespacesOfMtlLibReferences = true};

    private static readonly ObjMaterialFileReaderSettings ObjMaterialFileReaderSettings = new() {KeepWhitespacesOfMapFileReferences = true};

    public IModel3D? Parse(string fileName, Dictionary<string, Stream> files, ILogger? logger = null)
    {
        if (!files.TryGetValue(fileName, out var stream))
            return null;

        stream.Seek(0, SeekOrigin.Begin);
        var objFile = ObjFile.FromStream(stream, ObjFileReaderSettings);

        var objMaterialLibraries = CollectMaterialLibraries(objFile, files, logger).ToArray();

        var textures = CollectAvailableTextures(objMaterialLibraries)
            .Distinct()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(FileHandler.GetCleanedFileName!)
            .ToList();

        var fileInfos = new Dictionary<string, FileInformation>();
        foreach (var file in files)
        {
            fileInfos[file.Key] = new FileInformation
            {
                Name = file.Key
            };
        }

        fileInfos[fileName].Status = FileStatus.ReferencedGeometry;
        fileInfos[fileName].Data = stream.ToArray();

        foreach (var objMaterialLibrary in objMaterialLibraries)
        {
            if (fileInfos.TryGetValue(objMaterialLibrary.Item1, out var fileInfo))
            {
                fileInfo.Status = FileStatus.ReferencedMaterial;
                fileInfo.Data = files[fileInfo.Name].ToArray();
            }
            else
            {
                fileInfos[objMaterialLibrary.Item1] = new FileInformation
                {
                    Name = objMaterialLibrary.Item1,
                    Status = FileStatus.MissingMaterial
                };
            }
        }

        foreach (var texture in textures)
        {
            if (fileInfos.TryGetValue(texture, out var fileInfo))
            {
                fileInfo.Status = FileStatus.ReferencedTexture;
                fileInfo.Data = files[fileInfo.Name].ToArray();
            }
            else
            {
                fileInfos[texture] = new FileInformation
                {
                    Name = texture,
                    Status = FileStatus.MissingTexture
                };
            }
        }

        return new ObjModel3D
        {
            FileName = fileName,
            ReferencedMaterialLibraryFiles = objMaterialLibraries.Where(d => files.ContainsKey(d.Item1)).ToDictionary(d => d.Item1, d => files[d.Item1].ToArray()),
            ReferencedTextureFiles = textures.Where(files.ContainsKey).ToDictionary(d => d, d => files[d].ToArray()),
            Data = ConvertGeometry(objFile, objMaterialLibraries, fileInfos),
            ObjFile = stream.ToArray(),
            Files = fileInfos
        };
    }

    public IModel3D? Parse(string filePath, ILogger? logger = null)
    {
        var directory = Path.GetDirectoryName(filePath) ?? throw new ArgumentException($"The file directory of '{filePath}' could not be determined");
        var files = new Dictionary<string, Stream>();
        foreach (var fullPath in Directory.EnumerateFiles(directory))
        {
            if (TryGetFileStream(fullPath, out var stream))
                files[FileHandler.GetCleanedFileName(fullPath)] = stream;
        }

        try
        {
            return Parse(FileHandler.GetCleanedFileName(filePath), files, logger);
        }
        finally
        {
            foreach (var keyValuePair in files)
            {
                keyValuePair.Value.Dispose();
            }

            files.Clear();
        }
    }

    private static bool TryGetFileStream(string path, [NotNullWhen(true)] out Stream? stream)
    {
        try
        {
            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return true;
        }
        catch
        {
            stream = null;
            return false;
        }
    }

    private static IEnumerable<Tuple<string, ObjMaterialFile?>> CollectMaterialLibraries(ObjFile objFile, IReadOnlyDictionary<string, Stream> files, ILogger? logger)
        => objFile.MaterialLibraries.Select(mtl =>
        {
            try
            {
                mtl = FileHandler.GetCleanedFileName(mtl);
                if (!files.TryGetValue(mtl, out var materialFile))
                {
                    logger?.LogWarning("Material '{Name}' could not be loaded", mtl);
                    return new Tuple<string, ObjMaterialFile?>(mtl, null);
                }

                materialFile.Seek(0, SeekOrigin.Begin);
                return Tuple.Create<string, ObjMaterialFile?>(mtl, ObjMaterialFile.FromStream(materialFile, ObjMaterialFileReaderSettings));
            }
            catch (Exception e)
            {
                logger?.LogWarning(e, "Material '{Name}' could not be loaded", mtl);
                return new Tuple<string, ObjMaterialFile?>(mtl, null);
            }
        });

    private static IEnumerable<string?> CollectAvailableTextures(IEnumerable<Tuple<string, ObjMaterialFile?>> objMaterials)
    {
        foreach (var objMaterial in objMaterials.Where(mtl => mtl.Item2 is not null).SelectMany(materialFile => materialFile.Item2!.Materials))
        {
            yield return objMaterial.AmbientMap?.FileName;
            yield return objMaterial.BumpMap?.FileName;
            yield return objMaterial.DecalMap?.FileName;
            yield return objMaterial.DiffuseMap?.FileName;
            yield return objMaterial.DispMap?.FileName;
            yield return objMaterial.DissolveMap?.FileName;
            yield return objMaterial.ReflectionMap.CubeBack?.FileName;
            yield return objMaterial.ReflectionMap.CubeBottom?.FileName;
            yield return objMaterial.ReflectionMap.CubeFront?.FileName;
            yield return objMaterial.ReflectionMap.CubeLeft?.FileName;
            yield return objMaterial.ReflectionMap.CubeRight?.FileName;
            yield return objMaterial.ReflectionMap.CubeTop?.FileName;
            yield return objMaterial.EmissiveMap?.FileName;
            yield return objMaterial.SpecularMap?.FileName;
            yield return objMaterial.SpecularExponentMap?.FileName;
            yield return objMaterial.RoughnessMap?.FileName;
            yield return objMaterial.MetallicMap?.FileName;
            yield return objMaterial.SheenMap?.FileName;
            yield return objMaterial.Norm?.FileName;
        }
    }

    private static ModelData ConvertGeometry(ObjFile objFile, IEnumerable<Tuple<string, ObjMaterialFile?>> objMaterialFiles, IReadOnlyDictionary<string, FileInformation> files)
    {
        var materials = new List<ModelMaterial>(objMaterialFiles
            .Where(x => x.Item2 is not null)
            .SelectMany(x => x.Item2!.Materials)
            .Distinct()
            .Select(x => Convert(x, files)));
        var faceGroups = objFile.Groups.Count > 0
            ? objFile.Groups.Where(group => group.Faces.Count > 0).Select(group => ConvertGroup(group, materials)).ToList()
            : [CreateDefaultFaceGroup(objFile, materials)];

        return new ModelData
        {
            Vertices = [..objFile.Vertices.Select(vertex => new Vector3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z))],
            Normals = [..objFile.VertexNormals.Select(normal => new Vector3(normal.X, normal.Y, normal.Z))],
            TextureCoordinates = [..objFile.TextureVertices.Select(texture => new Vector2(texture.X, texture.Y))],
            FaceGroups = faceGroups,
            Materials = materials
        };
    }

    private static ModelFaceGroup CreateDefaultFaceGroup(ObjFile objFile, List<ModelMaterial> materials) => new()
    {
        Name = "Default",
        Faces = [..objFile.Faces.Select(face => ConvertFace(face, materials))]
    };

    private static ModelFaceGroup ConvertGroup(ObjGroup faceGroup, List<ModelMaterial> materials) => new()
    {
        Name = faceGroup.Name,
        Faces = [..faceGroup.Faces.Select(face => ConvertFace(face, materials))]
    };

    private static ModelFace ConvertFace(ObjFace face, List<ModelMaterial> materials)
    {
        var materialIndex = materials.FindIndex(material => material.Name == face.MaterialName);
        return new ModelFace
        {
            MaterialIndex = materialIndex,
            Vertices = [..face.Vertices.Select(Convert)]
        };
    }

    private static ModelFaceVertex Convert(ObjTriplet vertex) => new()
    {
        VertexIndex = vertex.Vertex,
        NormalIndex = vertex.Normal,
        TextureCoordinateIndex = vertex.Texture
    };

    private static ModelMaterial Convert(ObjMaterial objMaterial, IReadOnlyDictionary<string, FileInformation> files)
    {
        var material = new ModelMaterial
        {
            Name = objMaterial.Name ?? string.Empty,
            SpecularExponent = objMaterial.SpecularExponent,
            OpticalDensity = objMaterial.OpticalDensity,
            Dissolve = objMaterial.DissolveFactor,
            IlluminationModel = objMaterial.IlluminationModel,
            Metallic = objMaterial.Metallic,
            Roughness = objMaterial.Roughness,
            Sheen = objMaterial.Sheen,
            ClearCoatThickness = objMaterial.ClearCoatThickness,
            ClearCoatRoughness = objMaterial.ClearCoatRoughness,
            Anisotropy = objMaterial.Anisotropy,
            AnisotropyRotation = objMaterial.AnisotropyRotation
        };

        if (TryGetFilenameAndBytes(objMaterial.DiffuseMap, out var filename, out var content))
        {
            material.DiffuseTextureBytes = content;
        }

        material.DiffuseTextureName = filename ?? string.Empty;

        if (TryGetFilenameAndBytes(objMaterial.AmbientMap, out filename, out content))
        {
            material.AmbientTextureBytes = content;
        }

        material.AmbientTextureName = filename;

        if (TryGetFilenameAndBytes(objMaterial.SpecularMap, out filename, out content))
        {
            material.SpecularTextureBytes = content;
        }

        material.SpecularTextureName = filename;

        if (TryGetFilenameAndBytes(objMaterial.EmissiveMap, out filename, out content))
        {
            material.EmissiveTextureBytes = content;
        }

        material.EmissiveTextureName = filename;

        if (TryGetFilenameAndBytes(objMaterial.MetallicMap, out filename, out content))
        {
            material.MetallicTextureBytes = content;
        }

        material.MetallicTextureName = filename;

        if (TryGetFilenameAndBytes(objMaterial.RoughnessMap, out filename, out content))
        {
            material.RoughnessTextureBytes = content;
        }

        material.RoughnessTextureName = filename;

        if (TryGetFilenameAndBytes(objMaterial.SheenMap, out filename, out content))
        {
            material.SheenTextureBytes = content;
        }

        material.SheenTextureName = filename;

        if (TryGetFilenameAndBytes(objMaterial.Norm, out filename, out content))
        {
            material.NormTextureBytes = content;
        }

        material.NormTextureName = filename;

        if (TryGetColor(objMaterial.AmbientColor, out var color)) material.AmbientColor = color;
        if (TryGetColor(objMaterial.DiffuseColor, out color)) material.DiffuseColor = color!.Value;
        if (TryGetColor(objMaterial.SpecularColor, out color)) material.SpecularColor = color;
        if (TryGetColor(objMaterial.EmissiveColor, out color)) material.EmissiveColor = color;

        return material;

        bool TryGetFilenameAndBytes(ObjMaterialMap? map, out string? extractedFileName, out byte[] extractedContent)
        {
            extractedFileName = null;
            extractedContent = [];
            if (map is null) return false;
            if (string.IsNullOrWhiteSpace(map.FileName)) return false;
            extractedFileName = map.FileName ?? string.Empty;
            if (!files.TryGetValue(FileHandler.GetCleanedFileName(extractedFileName), out var fileInfo)) return false;
            if (fileInfo.Data is null) return false;
            extractedContent = fileInfo.Data!;
            return true;
        }

        static bool TryGetColor(ObjMaterialColor? objColor, out Vector3? colorVector)
        {
            colorVector = null;
            if (objColor is not null) colorVector = new Vector3(objColor.Color.X, objColor.Color.Y, objColor.Color.Z);
            return objColor is not null;
        }
    }
}