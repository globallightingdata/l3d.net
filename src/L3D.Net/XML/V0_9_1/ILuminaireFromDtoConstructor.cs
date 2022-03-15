using L3D.Net.Data;
using L3D.Net.XML.V0_9_1.Dto;

namespace L3D.Net.XML.V0_9_1
{
    interface ILuminaireFromDtoConstructor
    {
        Luminaire BuildLuminaireFromDto(LuminaireBuilder builder, LuminaireDto luminaireDto, string dataDirectory);
    }
}
