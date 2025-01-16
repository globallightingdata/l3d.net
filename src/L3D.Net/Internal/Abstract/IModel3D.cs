using L3D.Net.Data;
using System.Collections.Generic;

namespace L3D.Net.Internal.Abstract;

public interface IModel3D
{
    string FileName { get; set; }

    byte[] ObjFile { get; set; }

    Dictionary<string, byte[]> ReferencedMaterialLibraryFiles { get; set; }

    Dictionary<string, byte[]> ReferencedTextureFiles { get; set; }

    bool IsFaceIndexValid(int groupIndex, int faceIndex);

    ModelData? Data { get; }
}