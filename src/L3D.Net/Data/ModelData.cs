using System.Collections.Generic;
using System.Numerics;

namespace L3D.Net.Data;

public class ModelData
{
    public List<Vector3> Vertices { get; set; } = [];

    public List<Vector3> Normals { get; set; } = [];

    public List<Vector2> TextureCoordinates { get; set; } = [];

    public List<ModelFaceGroup> FaceGroups { get; set; } = [];

    public List<ModelMaterial> Materials { get; set; } = [];
}