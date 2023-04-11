using System;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using System.Collections.Generic;

namespace L3D.Net.Geometry;

public class ObjModel3D : IModel3D
{
    public string FileName { get; set; } = string.Empty;
    public byte[] ObjFile { get; set; } = Array.Empty<byte>();
    public ModelData? Data { get; set; }
    public Dictionary<string, byte[]> ReferencedMaterialLibraryFiles { get; set; } = new();
    public Dictionary<string, byte[]> ReferencedTextureFiles { get; set; } = new();
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