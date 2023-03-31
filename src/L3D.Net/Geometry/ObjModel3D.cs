using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Geometry;

public class ObjModel3D : IModel3D
{
    public string FileName { get; set; } = string.Empty;
    public Stream Stream { get; set; } = Stream.Null;
    public ModelData? Data { get; set; }
    public Dictionary<string, Stream> ReferencedMaterialLibraryFiles { get; set; } = new();
    public Dictionary<string, Stream> ReferencedTextureFiles { get; set; } = new();
    public bool IsFaceIndexValid(int groupIndex, int faceIndex)
    {
        if (Data == null)
            return false;

        if (groupIndex >= Data.FaceGroups.Count)
            return false;

        if (faceIndex >= Data.FaceGroups[groupIndex].Faces.Count)
            return false;

        return true;
    }
}