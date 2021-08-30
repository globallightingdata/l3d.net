using System;

namespace L3D.Net.Data
{
    internal class LightEmittingFaceRangeAssignment : LightEmittingFaceAssignment
    {
        public int FaceIndexBegin { get; }
        public int FaceIndexEnd { get; }

        public LightEmittingFaceRangeAssignment(string partName, int groupIndex, int faceIndexBegin, int faceIndexEnd) : base(partName, groupIndex)
        {
            if (faceIndexBegin < 0) throw new ArgumentOutOfRangeException(nameof(faceIndexBegin));
            if (faceIndexEnd < faceIndexBegin) throw new ArgumentOutOfRangeException(nameof(faceIndexEnd));
            FaceIndexBegin = faceIndexBegin;
            FaceIndexEnd = faceIndexEnd;
        }
    }
}