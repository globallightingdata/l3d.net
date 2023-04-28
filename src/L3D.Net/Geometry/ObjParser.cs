using JeremyAnsel.Media.WavefrontObj;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using L3D.Net.Extensions;

namespace L3D.Net.Geometry;

public class ObjParser : IObjParser
{
    public static readonly IObjParser Instance = new ObjParser();

    public IModel3D? Parse(string fileName, Dictionary<string, Stream> files, ILogger? logger = null)
    {
        if (!files.TryGetValue(fileName, out var stream))
            return null;

        stream.Seek(0, SeekOrigin.Begin);
        var objFile = ObjFile.FromStream(stream);

        var objMaterialLibraries = CollectAvailableMaterialLibraries(logger, objFile, files).ToList();

        List<string> textures = CollectAvailableTextures(objMaterialLibraries).Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).Where(files.ContainsKey!).ToList()!;

        return new ObjModel3D
        {
            FileName = fileName,
            ReferencedMaterialLibraryFiles = objMaterialLibraries.Select(tuple => tuple.Item1).ToDictionary(d => d, d => files[d].ToArray()),
            ReferencedTextureFiles = textures.ToDictionary(d => d, d => files[d].ToArray()),
            Data = ConvertGeometry(objFile, objMaterialLibraries.Select(tuple => tuple.Item2).ToList(), files.ToDictionary(d => d.Key, d => d.Value.ToArray())),
            ObjFile = stream.ToArray()
        };
    }

    public IModel3D Parse(string filePath, ILogger? logger = null)
    {
        var directory = Path.GetDirectoryName(filePath) ?? 
            throw new ArgumentException($"The file directory of '{filePath}' could not be determined");

        using var fs = File.OpenRead(filePath);

        var objFile = ObjFile.FromStream(fs);

        var objMaterialLibraries = CollectAvailableMaterialLibraries(objFile, directory, logger).ToList();

        List<string> textures = CollectAvailableTextures(objMaterialLibraries).Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList()!;

        var filePaths = objMaterialLibraries.Select(x => x.Item1).Union(textures).ToList();
        filePaths.Add(filePath);

        var files = filePaths.ToDictionary<string, string, byte[]>(Path.GetFileName, d => File.ReadAllBytes(Path.Combine(directory, d)));

        return new ObjModel3D
        {
            FileName = Path.GetFileName(filePath),
            ReferencedMaterialLibraryFiles = objMaterialLibraries.Select(tuple => tuple.Item1).ToDictionary(d => d, d => files[d]),
            ReferencedTextureFiles = textures.ToDictionary(d => d, d => files[d]),
            Data = ConvertGeometry(objFile, objMaterialLibraries.Select(tuple => tuple.Item2).ToList(), files),
            ObjFile = fs.ToArray()
        };
    }

    private static IEnumerable<Tuple<string, ObjMaterialFile>> CollectAvailableMaterialLibraries(ObjFile objFile, string directory, ILogger? logger = null)
    {
        return objFile.MaterialLibraries.Select(mtl =>
        {
            try
            {
                var materialFile = Path.Combine(directory, mtl);
                return Tuple.Create(mtl, ObjMaterialFile.FromFile(materialFile));
            }
            catch (Exception e)
            {
                logger?.Log(LogLevel.Warning, e.Message);
                return null;
            }
        }).Where(x => x != null)!;
    }

    private static IEnumerable<Tuple<string, ObjMaterialFile>> CollectAvailableMaterialLibraries(ILogger? logger,
        ObjFile objFile, IReadOnlyDictionary<string, Stream> files)
    {
        return objFile.MaterialLibraries.Select(mtl =>
        {
            try
            {
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
    }

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
            yield return objMaterial?.ReflectionMap?.CubeBack?.FileName;
            yield return objMaterial?.ReflectionMap?.CubeBottom?.FileName;
            yield return objMaterial?.ReflectionMap?.CubeFront?.FileName;
            yield return objMaterial?.ReflectionMap?.CubeLeft?.FileName;
            yield return objMaterial?.ReflectionMap?.CubeRight?.FileName;
            yield return objMaterial?.ReflectionMap?.CubeTop?.FileName;
            yield return objMaterial?.EmissiveMap?.FileName;
            yield return objMaterial?.SpecularMap?.FileName;
            yield return objMaterial?.SpecularExponentMap?.FileName;
        }
    }

    private static ModelData ConvertGeometry(ObjFile objFile, IEnumerable<ObjMaterialFile> objMaterialFiles, IReadOnlyDictionary<string, byte[]> files)
    {
        var vertices = objFile.Vertices
            .Select(vertex => new Vector3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z)).ToList();
        var normals = objFile.VertexNormals.Select(normal => new Vector3(normal.X, normal.Y, normal.Z)).ToList();
        var texCoords = objFile.TextureVertices.Select(texture => new Vector2(texture.X, texture.Y)).ToList();
        var materials = objMaterialFiles
            .SelectMany(file => file.Materials)
            .Distinct()
            .Select(x => Convert(x, files.ContainsKey(x.DiffuseMap?.FileName ?? string.Empty) ? files[x.DiffuseMap?.FileName!] : Array.Empty<byte>()))
            .ToList();
        var faceGroups = objFile.Groups.Any()
            ? objFile.Groups.Select(group => ConvertGroup(group, materials)).ToList()
            : new List<ModelFaceGroup> { CreateDefaultFaceGroup(objFile, materials) };

        return new ModelData
        {
            Vertices = vertices,
            Normals = normals,
            TextureCoordinates = texCoords,
            FaceGroups = faceGroups,
            Materials = materials
        };
    }

    private static ModelFaceGroup CreateDefaultFaceGroup(ObjFile objFile, List<ModelMaterial> materials)
    {
        var faces = objFile.Faces.Select(face => ConvertFace(face, materials));
        return new ModelFaceGroup
        {
            Name = "Default",
            Faces = faces.ToList()
        };
    }

    private static ModelFaceGroup ConvertGroup(ObjGroup faceGroup, List<ModelMaterial> materials)
    {
        var faces = faceGroup.Faces.Select(face => ConvertFace(face, materials)).ToList();
        return new ModelFaceGroup
        {
            Name = faceGroup.Name,
            Faces = faces.ToList()
        };
    }

    private static ModelFace ConvertFace(ObjFace face, List<ModelMaterial> materials)
    {
        List<ModelFaceVertex> vertices = new();
        var materialIndex = materials.FindIndex(material => material.Name == face.MaterialName);

        foreach (var vertex in face.Vertices)
        {
            vertices.Add(new ModelFaceVertex
            {
                VertexIndex = vertex.Vertex,
                NormalIndex = vertex.Normal,
                TextureCoordinateIndex = vertex.Texture
            });
        }

        return new ModelFace
        {
            MaterialIndex = materialIndex,
            Vertices = vertices
        };
    }

    private static ModelMaterial Convert(ObjMaterial objMaterial, byte[] bytes)
    {
        var color = objMaterial.DiffuseColor;
        return new ModelMaterial
        {
            Color = new Vector3(color.Color.X, color.Color.Y, color.Color.Z),
            Name = objMaterial.Name,
            TextureName = objMaterial.DiffuseMap?.FileName ?? string.Empty,
            TextureBytes = bytes
        };
    }
}