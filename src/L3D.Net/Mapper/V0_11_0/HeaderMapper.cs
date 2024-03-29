﻿using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Mapper.V0_11_0;

public class HeaderMapper : DtoMapperBase<Header, HeaderDto>
{
    public static readonly HeaderMapper Instance = new();

    protected override Header ConvertData(HeaderDto element) => new()
    {
        CreatedWithApplication = element.CreatedWithApplication,
        CreationTimeCode = element.CreationTimeCode,
        Description = element.Description,
        Name = element.Name,
        FormatVersion = FormatVersionMapper.Instance.Convert(element.FormatVersion)
    };

    protected override HeaderDto ConvertData(Header element) => new()
    {
        CreatedWithApplication = element.CreatedWithApplication,
        CreationTimeCode = element.CreationTimeCode,
        Description = element.Description,
        Name = element.Name,
        FormatVersion = FormatVersionMapper.Instance.Convert(element.FormatVersion)
    };
}