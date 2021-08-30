using System.Numerics;

namespace L3D.Net.API.Dto
{
    public class ModelDto
    {
        public Vector3[] Vertices { get; set; }
        public Vector3[] Normals { get; set; }
        public Vector2[] TextureCoordinates { get; set; }
        public FaceGroupDto[] FaceGroups { get; set; }
        public MaterialDto[] Materials { get; set; }
    }
}