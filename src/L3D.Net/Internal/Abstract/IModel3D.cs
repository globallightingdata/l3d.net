using L3D.Net.Data;
using System.Collections.Generic;

namespace L3D.Net.Internal.Abstract;

public interface IModel3D
{
    string FilePath { get; }
    IEnumerable<string> ReferencedMaterialLibraryFiles { get; }
    IEnumerable<string> ReferencedTextureFiles { get; }
    bool IsFaceIndexValid(int groupIndex, int faceIndex);
    ModelData Data { get; }
}