using System;
using System.Collections.Generic;

namespace L3D.Net.Data;

public class LightEmittingSurfacePart : Part
{
    public Dictionary<string, double> LightEmittingPartIntensityMapping { get; set; } = new();

    public List<FaceAssignment> FaceAssignments { get; set; } = new();

    public void AddLightEmittingObject(string leoPartName, double intensity = 1.0)
    {
        if (string.IsNullOrWhiteSpace(leoPartName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(leoPartName));

        if (intensity < 0) throw new ArgumentOutOfRangeException(nameof(intensity));
        if (intensity > 1) throw new ArgumentOutOfRangeException(nameof(intensity));

        LightEmittingPartIntensityMapping[leoPartName] = intensity;
    }

    public void AddFaceAssignment(int groupIndex, int faceIndex)
    {
        if (groupIndex < 0) throw new ArgumentOutOfRangeException(nameof(groupIndex));
        if (faceIndex < 0) throw new ArgumentOutOfRangeException(nameof(faceIndex));

        FaceAssignments.Add(new SingleFaceAssignment
        {
            FaceIndex = faceIndex,
            GroupIndex = groupIndex
        });
    }

    public void AddFaceAssignment(int groupIndex, int faceIndexBegin, int faceIndexEnd)
    {
        if (groupIndex < 0) throw new ArgumentOutOfRangeException(nameof(groupIndex));
        if (faceIndexBegin < 0) throw new ArgumentOutOfRangeException(nameof(faceIndexBegin));
        if (faceIndexEnd < 0) throw new ArgumentOutOfRangeException(nameof(faceIndexEnd));
        if (faceIndexEnd < faceIndexBegin) throw new ArgumentOutOfRangeException(nameof(faceIndexEnd));

        if (faceIndexBegin == faceIndexEnd)
            AddFaceAssignment(groupIndex, faceIndexBegin);
        else
            FaceAssignments.Add(new FaceRangeAssignment
            {
                GroupIndex = groupIndex,
                FaceIndexBegin = faceIndexBegin,
                FaceIndexEnd = faceIndexEnd
            });
    }
}