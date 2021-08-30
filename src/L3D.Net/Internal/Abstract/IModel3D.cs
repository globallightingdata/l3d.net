using System.Collections.Generic;
using L3D.Net.Data;

namespace L3D.Net.Internal.Abstract
{
    interface IModel3D
    {
        string FilePath { get; }
        IEnumerable<string> ReferencedMaterialLibraryFiles { get; }
        IEnumerable<string> ReferencedTextureFiles { get; }
        bool IsFaceIndexValid(int groupIndex, int faceIndex);
        ModelData Data { get; }
    }
}
