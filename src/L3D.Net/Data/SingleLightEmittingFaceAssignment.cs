using System;

namespace L3D.Net.Data
{
    internal class SingleLightEmittingFaceAssignment : LightEmittingFaceAssignment
    {
        public SingleLightEmittingFaceAssignment(string partName, int groupIndex, int faceIndex) : base(partName, groupIndex)
        {
            if (faceIndex < 0) throw new ArgumentOutOfRangeException(nameof(faceIndex));
            FaceIndex = faceIndex;
        }
        public int FaceIndex { get; }
    }
}