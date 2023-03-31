using L3D.Net.Data;
using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Internal.Abstract;

public interface IModel3D
{
    string FileName { get; set;  }
    Stream Stream { get; set; }
    Dictionary<string, Stream> ReferencedMaterialLibraryFiles { get; set; }
    Dictionary<string, Stream> ReferencedTextureFiles { get; set; }
    bool IsFaceIndexValid(int groupIndex, int faceIndex);
    ModelData? Data { get; }
}