using L3D.Net.Data;
using L3D.Net.XML.V0_9_2.Dto;
using System;

namespace L3D.Net.Mapper.V0_9_2
{
    public class GeometrySourceMapper : DtoMapperBase<GeometrySource, GeometrySourceDto>
    {
        public static readonly GeometrySourceMapper Instance = new();

        protected override GeometrySource ConvertData(GeometrySourceDto element) => element switch
        {
            GeometryReferenceDto geometryReference => new()
            {
                GeometryId = geometryReference.GeometryId
            },
            _ => throw new ArgumentOutOfRangeException(nameof(element))
        };

        protected override GeometrySourceDto ConvertData(GeometrySource element) => new GeometryReferenceDto
        {
            GeometryId = element.GeometryId
        };
    }
}
