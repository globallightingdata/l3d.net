using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace L3D.Net.Data;

class ModelData
{
    public IReadOnlyList<Vector3> Vertices { get; }

    public IReadOnlyList<Vector3> Normals { get; }

    public IReadOnlyList<Vector2> TextureCoordinates { get; }

    public IReadOnlyList<ModelFaceGroup> FaceGroups { get; }
    public IReadOnlyList<ModelMaterial> Materials { get; }

    public ModelData(IEnumerable<Vector3> vertices, IEnumerable<Vector3> normals, IEnumerable<Vector2> textureCoordinates, IEnumerable<ModelFaceGroup> faceGroups, IEnumerable<ModelMaterial> materials)
    {
        Vertices = vertices?.ToList() ?? throw new ArgumentNullException(nameof(vertices));
        Normals = normals?.ToList() ?? throw new ArgumentNullException(nameof(normals));
        TextureCoordinates = textureCoordinates?.ToList() ?? throw new ArgumentNullException(nameof(textureCoordinates));
        FaceGroups = faceGroups?.ToList() ?? throw new ArgumentNullException(nameof(faceGroups));
        Materials = materials?.ToArray() ?? throw new ArgumentNullException(nameof(materials));
    }
}