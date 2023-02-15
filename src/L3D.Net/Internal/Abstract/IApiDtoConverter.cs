using L3D.Net.API.Dto;
using L3D.Net.Data;

namespace L3D.Net.Internal.Abstract;

interface IApiDtoConverter
{
    LuminaireDto Convert(Luminaire luminaire, string directory);
}