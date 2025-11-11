using System.Collections.Generic;
using System.Linq;
using L3D.Net.Data;
using L3D.Net.Internal;

namespace L3D.Net.Extensions;

public static class ModelDataExtensions
{
    public static IEnumerable<string> GetReferencedTextureFiles(this ModelData modelData) =>
        modelData.Materials.SelectMany(modelDataMaterial => modelDataMaterial.GetReferencedTextureFiles()).Select(FileHandler.GetCleanedFileName);
}