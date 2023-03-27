using L3D.Net.XML.V0_9_2.Dto;
using System.Numerics;

namespace L3D.Net.Mapper.V0_9_2
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
