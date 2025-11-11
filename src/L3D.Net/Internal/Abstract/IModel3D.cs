using L3D.Net.Data;
using System.Collections.Generic;

namespace L3D.Net.Internal.Abstract;

public interface IModel3D
{
    public string FileName { get; set; }

    public byte[] ObjFile { get; set; }

    public Dictionary<string, byte[]> ReferencedMaterialLibraryFiles { get; set; }

    public Dictionary<string, byte[]> ReferencedTextureFiles { get; set; }

    public bool IsFaceIndexValid(int groupIndex, int faceIndex);

    public ModelData? Data { get; }

    public Dictionary<string, FileInformation> Files { get; set; }
}