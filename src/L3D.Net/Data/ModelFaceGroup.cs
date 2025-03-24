using System.Collections.Generic;

namespace L3D.Net.Data;

public class ModelFaceGroup
{
    public string Name { get; set; } = string.Empty;

    public List<ModelFace> Faces { get; set; } = new();
}