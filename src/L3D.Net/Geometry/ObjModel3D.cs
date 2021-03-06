using System.Collections.Generic;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;

namespace L3D.Net.Geometry
{
    internal class ObjModel3D : IModel3D
    {
        public string FilePath { get; set; }

        IEnumerable<string> IModel3D.ReferencedTextureFiles => ReferencedTextureFiles;
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

        public ModelData Data { get; set; }

        IEnumerable<string> IModel3D.ReferencedMaterialLibraryFiles => ReferencedMaterialLibraries;

        public List<string> ReferencedMaterialLibraries { get; set; } = new List<string>();
        public List<string> ReferencedTextureFiles { get; set; } = new List<string>();
    }
}