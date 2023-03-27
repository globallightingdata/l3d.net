using L3D.Net.Data;
using L3D.Net.XML.V0_9_2.Dto;
using System;

namespace L3D.Net.Mapper.V0_9_2
{
    public class GeometryDefinitionMapper : DtoMapperBase<GeometrySource, GeometryDefinitionDto>
    {
        public static readonly GeometryDefinitionMapper Instance = new();

        protected override GeometrySource ConvertData(GeometryDefinitionDto element) => element switch
        {
            GeometryFileDefinitionDto geometryFileDefinition => new GeometrySource
            {
                GeometryId = geometryFileDefinition.Id,
                FileName = geometryFileDefinition.FileName,
                Units = GeometricUnitsMapper.Instance.Convert(geometryFileDefinition.Units)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(element))
        };

        protected override GeometryDefinitionDto ConvertData(GeometrySource element) =>
            new GeometryFileDefinitionDto
            {
                Id = element.GeometryId,
                FileName = element.FileName,
                Units = GeometricUnitsMapper.Instance.Convert(element.Units)
            };
    }
}
