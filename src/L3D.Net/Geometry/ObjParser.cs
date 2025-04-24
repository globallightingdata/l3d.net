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

namespace L3D.Net.Geometry;

public class ObjParser : IObjParser
{
    public static readonly ObjParser Instance = new();
    private static readonly ObjFileReaderSettings DefaultSettings = new() {HandleObjectNamesAsGroup = true, OnlyOneGroupNamePerLine = true};

    private static string GetFileName(string path)
    {
        var stage1 = path.Split('\\').LastOrDefault();
        if (string.IsNullOrEmpty(stage1))
            return path;
        var stage2 = stage1.Split('/').LastOrDefault();
        if (string.IsNullOrEmpty(stage2))
            return path;
        return stage2;
    }

    public IModel3D? Parse(string fileName, Dictionary<string, Stream> files, ILogger? logger = null)
    {
        if (!files.TryGetValue(fileName, out var stream))
            return null;

        stream.Seek(0, SeekOrigin.Begin);
        var objFile = ObjFile.FromStream(stream, DefaultSettings);

        var objMaterialLibraries = CollectAvailableMaterialLibraries(logger, objFile, files).ToList();

        var textures = CollectAvailableTextures(objMaterialLibraries)
            .Distinct()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(GetFileName!)
            .Where(files.ContainsKey)
            .ToList();

        return new ObjModel3D
        {
            FileName = fileName,
            ReferencedMaterialLibraryFiles = objMaterialLibraries.ToDictionary(d => d.Item1, d => files[d.Item1].ToArray()),
            ReferencedTextureFiles = textures.ToDictionary(d => d, d => files[d].ToArray()),
            Data = ConvertGeometry(objFile, objMaterialLibraries.Select(tuple => tuple.Item2), files.ToDictionary(d => d.Key, d => d.Value.ToArray())),
            ObjFile = stream.ToArray()
        };
    }

    public IModel3D Parse(string filePath, ILogger? logger = null)
    {
        var directory = Path.GetDirectoryName(filePath) ?? throw new ArgumentException($"The file directory of '{filePath}' could not be determined");

        ObjFile objFile;
        byte[] fileBytes;
        using (var fs = File.OpenRead(filePath))
        {
            objFile = ObjFile.FromStream(fs, DefaultSettings);
            fileBytes = fs.ToArray();
        }

        var objMaterialLibraries = CollectAvailableMaterialLibraries(objFile, directory, logger).ToList();
        var textures = CollectAvailableTextures(objMaterialLibraries).Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).Select(GetFileName!).ToList();
        var filePaths = objMaterialLibraries.Select(x => x.Item1).Union(textures).ToList();
        filePaths.Add(filePath);

        var files = filePaths.ToDictionary(GetFileName, d => File.ReadAllBytes(Path.Combine(directory, d)));

        return new ObjModel3D
        {
            FileName = GetFileName(filePath),
            ReferencedMaterialLibraryFiles = objMaterialLibraries.Select(tuple => tuple.Item1).ToDictionary(d => d, d => files[d]),
            ReferencedTextureFiles = textures.ToDictionary(d => d, d => files[d]),
            Data = ConvertGeometry(objFile, objMaterialLibraries.Select(tuple => tuple.Item2).ToList(), files),
            ObjFile = fileBytes
        };
    }

    private static IEnumerable<Tuple<string, ObjMaterialFile>> CollectAvailableMaterialLibraries(ObjFile objFile, string directory, ILogger? logger = null)
        => objFile.MaterialLibraries.Select(mtl =>
        {
            try
            {
                mtl = GetFileName(mtl);
                var materialFile = Path.Combine(directory, mtl);
                return Tuple.Create(mtl, ObjMaterialFile.FromFile(materialFile));
            }
            catch (Exception e)
            {
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                logger?.Log(LogLevel.Warning, e.Message);
                return null;
            }
        }).Where(x => x != null)!;

    private static IEnumerable<Tuple<string, ObjMaterialFile>> CollectAvailableMaterialLibraries(ILogger? logger, ObjFile objFile, IReadOnlyDictionary<string, Stream> files)
        => objFile.MaterialLibraries.Select(mtl =>
        {
            try
            {
                mtl = GetFileName(mtl);
                var materialFile = files[mtl];
                materialFile.Seek(0, SeekOrigin.Begin);
                return Tuple.Create(mtl, ObjMaterialFile.FromStream(materialFile));
            }
            catch (Exception e)
            {
                logger?.LogWarning(e, "Material could not be loaded");
                return null;
            }
        }).Where(x => x != null)!;

    private static IEnumerable<string?> CollectAvailableTextures(IEnumerable<Tuple<string, ObjMaterialFile>> objMaterials)
    {
        foreach (var objMaterial in objMaterials.SelectMany(materialFile => materialFile.Item2.Materials))
        {
            yield return objMaterial?.AmbientMap?.FileName;
            yield return objMaterial?.BumpMap?.FileName;
            yield return objMaterial?.DecalMap?.FileName;
            yield return objMaterial?.DiffuseMap?.FileName;
            yield return objMaterial?.DispMap?.FileName;
            yield return objMaterial?.DissolveMap?.FileName;
            yield return objMaterial?.ReflectionMap.CubeBack?.FileName;
            yield return objMaterial?.ReflectionMap.CubeBottom?.FileName;
            yield return objMaterial?.ReflectionMap.CubeFront?.FileName;
            yield return objMaterial?.ReflectionMap.CubeLeft?.FileName;
            yield return objMaterial?.ReflectionMap.CubeRight?.FileName;
            yield return objMaterial?.ReflectionMap.CubeTop?.FileName;
            yield return objMaterial?.EmissiveMap?.FileName;
            yield return objMaterial?.SpecularMap?.FileName;
            yield return objMaterial?.SpecularExponentMap?.FileName;
        }
    }

    private static ModelData ConvertGeometry(ObjFile objFile, IEnumerable<ObjMaterialFile> objMaterialFiles, IReadOnlyDictionary<string, byte[]> files)
    {
        var materials = objMaterialFiles
            .SelectMany(file => file.Materials)
            .Distinct()
            .Select(x => Convert(x, files.TryGetValue(x.DiffuseMap?.FileName ?? string.Empty, out var b) ? b : []))
            .ToList();
        var faceGroups = objFile.Groups.Count > 0
            ? objFile.Groups.Select(group => ConvertGroup(group, materials)).ToList()
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

    private static ModelMaterial Convert(ObjMaterial objMaterial, byte[] bytes)
    {
        var material = new ModelMaterial
        {
            Name = objMaterial.Name ?? string.Empty,
            TextureName = objMaterial.DiffuseMap?.FileName ?? string.Empty,
            TextureBytes = bytes
        };
        var color = objMaterial.DiffuseColor;
        if (color is not null) material.Color = new Vector3(color.Color.X, color.Color.Y, color.Color.Z);
        return material;
    }
}