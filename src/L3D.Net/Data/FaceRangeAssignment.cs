using System;

namespace L3D.Net.Data
{
    internal class FaceRangeAssignment : FaceAssignment
    {
        public int FaceIndexBegin { get; }
        public int FaceIndexEnd { get; }

        public FaceRangeAssignment(int groupIndex, int faceIndexBegin, int faceIndexEnd) : base(groupIndex)
        {
            if (faceIndexBegin < 0) throw new ArgumentOutOfRangeException(nameof(faceIndexBegin));
            if (faceIndexEnd < faceIndexBegin) throw new ArgumentOutOfRangeException(nameof(faceIndexEnd));
            FaceIndexBegin = faceIndexBegin;
            FaceIndexEnd = faceIndexEnd;
        }
    }
}