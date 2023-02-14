using System.Numerics;

namespace L3D.Net.API.Dto;

public class MaterialDto
{
    public string Name { get; set; }
    public Vector3 Color { get; set; }
    public string TextureName { get; set; }
    public byte[] TextureBytes { get; set; }
}