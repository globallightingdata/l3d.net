using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.Data;

public class ModelData
{
    public List<Vector3> Vertices { get; set; } = new();

    public List<Vector3> Normals { get; set; } = new();

    public List<Vector2> TextureCoordinates { get; set; } = new();

    public List<ModelFaceGroup> FaceGroups { get; set; } = new();
    public List<ModelMaterial> Materials { get; set; } = new();
}