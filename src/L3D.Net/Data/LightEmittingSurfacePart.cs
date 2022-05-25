using System;
using System.Collections.Generic;

namespace L3D.Net.Data
{
    internal class LightEmittingSurfacePart : Part
    {
        private readonly Dictionary<string, double> _lightEmittingPartIntensityMapping = new();
        private readonly List<FaceAssignment> _faceAssignments = new();

        public LightEmittingSurfacePart(string partName) : base(partName)
        {
        }

        public IReadOnlyDictionary<string, double> LightEmittingPartIntensityMapping =>
            _lightEmittingPartIntensityMapping;

        public IEnumerable<FaceAssignment> FaceAssignments => _faceAssignments;

        public void AddLightEmittingObject(string leoPartName, double intensity = 1.0)
        {
            if (string.IsNullOrWhiteSpace(leoPartName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(leoPartName));

            if (intensity < 0) throw new ArgumentOutOfRangeException(nameof(intensity));
            if (intensity > 1) throw new ArgumentOutOfRangeException(nameof(intensity));

            _lightEmittingPartIntensityMapping[leoPartName] = intensity;
        }

        public void AddFaceAssignment(int groupIndex, int faceIndex)
        {
            if (groupIndex < 0) throw new ArgumentOutOfRangeException(nameof(groupIndex));
            if (faceIndex < 0) throw new ArgumentOutOfRangeException(nameof(faceIndex));

            _faceAssignments.Add(new SingleFaceAssignment(groupIndex, faceIndex));
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
                _faceAssignments.Add(new FaceRangeAssignment(groupIndex, faceIndexBegin, faceIndexEnd));
        }
    }
}