using System;

namespace L3D.Net.Data;

internal abstract class FaceAssignment
{
    public FaceAssignment(int groupIndex)
    {
        if (groupIndex < 0) throw new ArgumentOutOfRangeException(nameof(groupIndex));

        GroupIndex = groupIndex;
    }

    public int GroupIndex { get; }
}