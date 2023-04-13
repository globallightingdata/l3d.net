using L3D.Net.Data;
using System;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Mapper.V0_11_0
{
    public class GeometryDefinitionMapper : DtoMapperBase<GeometryFileDefinition, GeometryDefinitionDto>
    {
        public static readonly GeometryDefinitionMapper Instance = new();

        protected override GeometryFileDefinition ConvertData(GeometryDefinitionDto element) => element switch
        {
            GeometryFileDefinitionDto geometryFileDefinition => new GeometryFileDefinition
            {
                GeometryId = geometryFileDefinition.GeometryId,
                FileName = geometryFileDefinition.FileName,
                Units = GeometricUnitsMapper.Instance.Convert(geometryFileDefinition.Units)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(element))
        };

        protected override GeometryDefinitionDto ConvertData(GeometryFileDefinition element) =>
            new GeometryFileDefinitionDto
            {
                GeometryId = element.GeometryId,
                FileName = element.FileName,
                Units = GeometricUnitsMapper.Instance.Convert(element.Units)
            };
    }
}
