using System;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.Data
{
    class ModelFace
    {
        public ModelFace(IEnumerable<ModelFaceVertex> vertices, int materialIndex)
        {
            Vertices = vertices?.ToList() ?? throw new ArgumentNullException(nameof(vertices));
            MaterialIndex = materialIndex;
        }

        public IReadOnlyList<ModelFaceVertex> Vertices { get; }
        public int MaterialIndex { get; }
    }
}