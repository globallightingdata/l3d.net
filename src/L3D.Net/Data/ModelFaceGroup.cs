using System;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.Data;

class ModelFaceGroup
{
    public ModelFaceGroup(string name, IEnumerable<ModelFace> faces)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(name));

        Name = name;
        Faces = faces?.ToList() ?? throw new ArgumentNullException(nameof(faces));
    }

    public string Name { get; }
    public IReadOnlyList<ModelFace> Faces { get; }
}