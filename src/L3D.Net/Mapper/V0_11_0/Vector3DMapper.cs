using System.Numerics;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Mapper.V0_11_0
{
    public class Vector3DMapper : DtoMapperBase<Vector3, Vector3Dto>
    {
        public static readonly Vector3DMapper Instance = new();

        protected override Vector3 ConvertData(Vector3Dto element) => new()
        {
            X = element.X,
            Y = element.Y,
            Z = element.Z
        };

        protected override Vector3Dto ConvertData(Vector3 element) => new()
        {
            X = element.X,
            Y = element.Y,
            Z = element.Z
        };
    }
}
