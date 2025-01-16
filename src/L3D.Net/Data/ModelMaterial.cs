using System;
using System.Numerics;

namespace L3D.Net.Data;

public class ModelMaterial
{
    public string Name { get; set; } = string.Empty;

    public Vector3 Color { get; set; }

    public string TextureName { get; set; } = string.Empty;

    public byte[] TextureBytes { get; set; } = Array.Empty<byte>();
}