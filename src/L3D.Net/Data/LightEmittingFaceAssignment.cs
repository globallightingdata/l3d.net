using System;

namespace L3D.Net.Data
{
    internal abstract class LightEmittingFaceAssignment
    {
        public LightEmittingFaceAssignment(string partName, int groupIndex)
        {
            if (string.IsNullOrWhiteSpace(partName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(partName));

            if (groupIndex < 0) throw new ArgumentOutOfRangeException(nameof(groupIndex));

            PartName = partName;
            GroupIndex = groupIndex;
        }

        public string PartName { get; }
        public int GroupIndex { get; }
    }
}