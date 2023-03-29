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

    public IModel3D Parse(string filePath, ILogger? logger = null)
    {
        var directory = Path.GetDirectoryName(filePath) ??
                        throw new ArgumentException($"The file directory of '{filePath}' could not be determined!");

        var objFile = ObjFile.FromFile(filePath);

        var objMaterialLibraries = CollectAvailableMaterialLibraries(logger, objFile, directory);

        var textures = CollectAvailableTextures(directory, objMaterialLibraries);

        return new ObjModel3D
        {
            FilePath = filePath,
            ReferencedMaterialLibraries = objMaterialLibraries.Select(tuple => tuple.Item1).ToList(),
            ReferencedTextureFiles = textures,
            Data = ConvertGeometry(objFile, objMaterialLibraries.Select(tuple => tuple.Item2).ToList())
        };
    }

    private static List<Tuple<string, ObjMaterialFile>> CollectAvailableMaterialLibraries(ILogger? logger,
        ObjFile objFile, string directory)
    {
        var objMaterials = objFile.MaterialLibraries.Select(mtl =>
        {
            try
            {
                var materialFile = Path.Combine(directory, mtl);
                return Tuple.Create(materialFile, ObjMaterialFile.FromFile(materialFile));
            }
            catch (Exception e)
            {
                logger?.LogWarning(e, "Material could not be loaded");
                return null!;
            }
        }).Where(mtl => mtl != null).ToList();
        return objMaterials;
    }

    private static List<string> CollectAvailableTextures(string directory,
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
            .Distinct()
            .Select(fileName => Path.Combine(directory, fileName!))
            .ToList()!;
        return textures!;
    }


    private static ModelData ConvertGeometry(ObjFile objFile, IReadOnlyList<ObjMaterialFile> objMaterialFiles)
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