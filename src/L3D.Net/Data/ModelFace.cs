using System.Collections.Generic;

namespace L3D.Net.Data;

public class ModelFace
{
    public List<ModelFaceVertex> Vertices { get; set; } = new();

    public int MaterialIndex { get; set; }
}