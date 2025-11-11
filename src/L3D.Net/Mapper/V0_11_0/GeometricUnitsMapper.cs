using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;
using System;

namespace L3D.Net.Mapper.V0_11_0;

public sealed class GeometricUnitsMapper : DtoMapperBase<GeometricUnits, GeometricUnitsDto>
{
    public static readonly GeometricUnitsMapper Instance = new();

    protected override GeometricUnits ConvertData(GeometricUnitsDto element) => element switch
    {
        GeometricUnitsDto.m => GeometricUnits.m,
        GeometricUnitsDto.dm => GeometricUnits.dm,
        GeometricUnitsDto.cm => GeometricUnits.cm,
        GeometricUnitsDto.mm => GeometricUnits.mm,
        GeometricUnitsDto.yard => GeometricUnits.yard,
        GeometricUnitsDto.foot => GeometricUnits.foot,
        GeometricUnitsDto.inch => GeometricUnits.inch,
        _ => throw new ArgumentOutOfRangeException(nameof(element))
    };

    protected override GeometricUnitsDto ConvertData(GeometricUnits element) => element switch
    {
        GeometricUnits.m => GeometricUnitsDto.m,
        GeometricUnits.dm => GeometricUnitsDto.dm,
        GeometricUnits.cm => GeometricUnitsDto.cm,
        GeometricUnits.mm => GeometricUnitsDto.mm,
        GeometricUnits.yard => GeometricUnitsDto.yard,
        GeometricUnits.foot => GeometricUnitsDto.foot,
        GeometricUnits.inch => GeometricUnitsDto.inch,
        _ => throw new ArgumentOutOfRangeException(nameof(element))
    };
}