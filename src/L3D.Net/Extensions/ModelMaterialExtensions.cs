using System.Collections.Generic;
using L3D.Net.Data;

namespace L3D.Net.Extensions;

public static class ModelMaterialExtensions
{
    public static IEnumerable<string> GetReferencedTextureFiles(this ModelMaterial material)
    {
        if (!string.IsNullOrWhiteSpace(material.AmbientTextureName)) yield return material.AmbientTextureName!;
        if (!string.IsNullOrWhiteSpace(material.DiffuseTextureName)) yield return material.DiffuseTextureName;
        if (!string.IsNullOrWhiteSpace(material.EmissiveTextureName)) yield return material.EmissiveTextureName!;
        if (!string.IsNullOrWhiteSpace(material.MetallicTextureName)) yield return material.MetallicTextureName!;
        if (!string.IsNullOrWhiteSpace(material.NormTextureName)) yield return material.NormTextureName!;
        if (!string.IsNullOrWhiteSpace(material.RoughnessTextureName)) yield return material.RoughnessTextureName!;
        if (!string.IsNullOrWhiteSpace(material.SheenTextureName)) yield return material.SheenTextureName!;
        if (!string.IsNullOrWhiteSpace(material.SpecularTextureName)) yield return material.SpecularTextureName!;
    }
}