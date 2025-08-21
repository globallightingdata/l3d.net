using System;
using System.Numerics;

namespace L3D.Net.Data;

public class ModelMaterial
{
    public string Name { get; set; } = string.Empty;

    public Vector3 DiffuseColor { get; set; }

    [Obsolete("Use " + nameof(DiffuseColor) + " instead")]
    public Vector3 Color
    {
        get => DiffuseColor;
        set => DiffuseColor = value;
    }

    public string DiffuseTextureName { get; set; } = string.Empty;

    [Obsolete("Use " + nameof(DiffuseTextureName) + " instead")]
    public string TextureName
    {
        get => DiffuseTextureName;
        set => DiffuseTextureName = value;
    }

    public byte[] DiffuseTextureBytes { get; set; } = [];

    [Obsolete("Use " + nameof(DiffuseTextureBytes) + " instead")]
    public byte[] TextureBytes
    {
        get => DiffuseTextureBytes;
        set => DiffuseTextureBytes = value;
    }

    public Vector3? AmbientColor { get; set; }

    public string? AmbientTextureName { get; set; }

    public byte[]? AmbientTextureBytes { get; set; }

    public Vector3? SpecularColor { get; set; }

    public string? SpecularTextureName { get; set; }

    public byte[]? SpecularTextureBytes { get; set; }

    public Vector3? EmissiveColor { get; set; }

    public string? EmissiveTextureName { get; set; }

    public byte[]? EmissiveTextureBytes { get; set; }

    public float Dissolve { get; set; }

    public float SpecularExponent { get; set; }

    public float OpticalDensity { get; set; }

    public int IlluminationModel { get; set; }

    #region PBR Extensions

    public float Metallic { get; set; }

    public string? MetallicTextureName { get; set; }

    public byte[]? MetallicTextureBytes { get; set; }

    public float Roughness { get; set; }

    public string? RoughnessTextureName { get; set; }

    public byte[]? RoughnessTextureBytes { get; set; }

    public float Sheen { get; set; }

    public string? SheenTextureName { get; set; }

    public byte[]? SheenTextureBytes { get; set; }

    public string? NormTextureName { get; set; }

    public byte[]? NormTextureBytes { get; set; }

    public float ClearCoatThickness { get; set; }

    public float ClearCoatRoughness { get; set; }

    public float Anisotropy { get; set; }

    public float AnisotropyRotation { get; set; }

    #endregion
}