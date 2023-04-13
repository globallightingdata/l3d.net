using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Mapper.V0_11_0
{
    public class GeometryReferenceMapper : DtoMapperBase<GeometryFileDefinition, GeometryReferenceDto>
    {
        public static readonly GeometryReferenceMapper Instance = new();

        protected override GeometryFileDefinition ConvertData(GeometryReferenceDto element) => new()
        {
            GeometryId = element.GeometryId
        };

        protected override GeometryReferenceDto ConvertData(GeometryFileDefinition element) => new()
        {
            GeometryId = element.GeometryId
        };
    }
}
