using System;

namespace L3D.Net.Data;

internal class SingleFaceAssignment : FaceAssignment
{
    public SingleFaceAssignment(int groupIndex, int faceIndex) : base(groupIndex)
    {
        if (faceIndex < 0) throw new ArgumentOutOfRangeException(nameof(faceIndex));
        FaceIndex = faceIndex;
    }
    public int FaceIndex { get; }
}