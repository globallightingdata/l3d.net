using JeremyAnsel.Media.WavefrontObj;
using L3D.Net.Data;
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
    public static readonly IObjParser Instance = new ObjParser();

    public IModel3D? Parse(string fileName, Dictionary<string, Stream> files, ILogger? logger = null)
    {
        if (!files.TryGetValue(fileName, out var stream))
            return null;

        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        copy.Seek(0, SeekOrigin.Begin);
        var objFile = ObjFile.FromStream(copy);

        var objMaterialLibraries = CollectAvailableMaterialLibraries(logger, objFile, files);

        var textures = CollectAvailableTextures(files, objMaterialLibraries);

        return new ObjModel3D
        {
            FileName = fileName,
            ReferencedMaterialLibraryFiles = objMaterialLibraries.Select(tuple => tuple.Item1).ToDictionary(d => d, d =>
            {
                var data = new byte[files[d].Length];
                using var br = new BinaryReader(files[d]);
                data = br.ReadBytes(data.Length);
                return data;
            }),
            ReferencedTextureFiles = textures.ToDictionary(d => d, d =>
            {
                var data = new byte[files[d].Length];
                using var br = new BinaryReader(files[d]);
                data = br.ReadBytes(data.Length);
                return data;
            }),
            Data = ConvertGeometry(objFile, objMaterialLibraries.Select(tuple => tuple.Item2).ToList())
        };
    }

    public IModel3D Parse(string filePath, ILogger? logger = null)
    {
        var directory = Path.GetDirectoryName(filePath) ??
                        throw new ArgumentException($"The file directory of '{filePath}' could not be determined");

        var objFile = ObjFile.FromFile(filePath);

        var objMaterialLibraries = CollectAvailableMaterialLibraries(objFile, directory, logger);

        var textures = CollectAvailableTextures(objMaterialLibraries);

        var filePaths = objMaterialLibraries.Select(x => x.Item1).Union(textures).ToList();
        filePaths.Add(filePath);

        var files = filePaths.ToDictionary<string, string, byte[]>(Path.GetFileName, d => File.ReadAllBytes(Path.Combine(directory, d)));

        return new ObjModel3D
        {
            FileName = Path.GetFileName(filePath),
            ReferencedMaterialLibraryFiles = objMaterialLibraries.Select(tuple => tuple.Item1).ToDictionary(d => d, d => files[d]),
            ReferencedTextureFiles = textures.ToDictionary(d => d, d => files[d]),
            Data = ConvertGeometry(objFile, objMaterialLibraries.Select(tuple => tuple.Item2).ToList())
        };
    }

    private static List<Tuple<string, ObjMaterialFile>> CollectAvailableMaterialLibraries(ObjFile objFile, string directory, ILogger? logger = null)
    {
        List<Tuple<string, ObjMaterialFile>> objMaterials = objFile.MaterialLibraries.Select(mtl =>
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
        }).Where(mtl => mtl != null).ToList()!;
        return objMaterials;
    }

    private static List<Tuple<string, ObjMaterialFile>> CollectAvailableMaterialLibraries(ILogger? logger,
        ObjFile objFile, IReadOnlyDictionary<string, Stream> files)
    {
        var objMaterials = objFile.MaterialLibraries.Select(mtl =>
        {
            try
            {
                var materialFile = files[mtl];
                using var copy = new MemoryStream();
                materialFile.CopyTo(copy);
                copy.Seek(0, SeekOrigin.Begin);
                return Tuple.Create(mtl, ObjMaterialFile.FromStream(copy));
            }
            catch (Exception e)
            {
                logger?.LogWarning(e, "Material could not be loaded");
                return null!;
            }
        }).Where(mtl => mtl != null).ToList();
        return objMaterials;
    }

    private static List<string> CollectAvailableTextures(List<Tuple<string, ObjMaterialFile>> objMaterials)
    {
        var textures = new List<string?>();

        foreach (var materialFile in objMaterials)
        {
            foreach (var objMaterial in materialFile.Item2.Materials)
            {
                textures.Add(objMaterial?.AmbientMap?.FileName);
                textures.Add(objMaterial?.BumpMap?.FileName);
                textures.Add(objMaterial?.DecalMap?.FileName);
                textures.Add(objMaterial?.DiffuseMap?.FileName);
                textures.Add(objMaterial?.DispMap?.FileName);
                textures.Add(objMaterial?.DissolveMap?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeBack?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeBottom?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeFront?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeLeft?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeRight?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeTop?.FileName);
                textures.Add(objMaterial?.EmissiveMap?.FileName);
                textures.Add(objMaterial?.SpecularMap?.FileName);
                textures.Add(objMaterial?.SpecularExponentMap?.FileName);
            }
        }

        textures = textures
            .Where(texture => texture != null)
            .Distinct()
            .ToList();
        return textures!;
    }

    private static IEnumerable<string> CollectAvailableTextures(IReadOnlyDictionary<string, Stream> files,
        List<Tuple<string, ObjMaterialFile>> objMaterials)
    {
        var textures = new List<string?>();

        foreach (var materialFile in objMaterials)
        {
            foreach (var objMaterial in materialFile.Item2.Materials)
            {
                textures.Add(objMaterial?.AmbientMap?.FileName);
                textures.Add(objMaterial?.BumpMap?.FileName);
                textures.Add(objMaterial?.DecalMap?.FileName);
                textures.Add(objMaterial?.DiffuseMap?.FileName);
                textures.Add(objMaterial?.DispMap?.FileName);
                textures.Add(objMaterial?.DissolveMap?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeBack?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeBottom?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeFront?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeLeft?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeRight?.FileName);
                textures.Add(objMaterial?.ReflectionMap?.CubeTop?.FileName);
                textures.Add(objMaterial?.EmissiveMap?.FileName);
                textures.Add(objMaterial?.SpecularMap?.FileName);
                textures.Add(objMaterial?.SpecularExponentMap?.FileName);
            }
        }

        textures = textures
            .Where(texture => texture != null)
            .Where(files.ContainsKey!)
            .Distinct()
            .ToList();
        return textures!;
    }


    private static ModelData ConvertGeometry(ObjFile objFile, IEnumerable<ObjMaterialFile> objMaterialFiles)
    {
        var vertices = objFile.Vertices
            .Select(vertex => new Vector3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z)).ToList();
        var normals = objFile.VertexNormals.Select(normal => new Vector3(normal.X, normal.Y, normal.Z)).ToList();
        var texCoords = objFile.TextureVertices.Select(texture => new Vector2(texture.X, texture.Y)).ToList();
        var materials = objMaterialFiles.SelectMany(file => file.Materials).Distinct().Select(Convert).ToList();
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

    private static ModelMaterial Convert(ObjMaterial objMaterial)
    {
        var color = objMaterial.DiffuseColor;
        return new ModelMaterial
        {
            Color = new Vector3(color.Color.X, color.Color.Y, color.Color.Z),
            Name = objMaterial.Name,
            TextureName = objMaterial.DiffuseMap?.FileName ?? string.Empty
        };
    }
}