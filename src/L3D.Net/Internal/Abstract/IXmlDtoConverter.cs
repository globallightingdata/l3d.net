using L3D.Net.Data;
using L3D.Net.XML.V0_9_2.Dto;

namespace L3D.Net.Internal.Abstract;

interface IXmlDtoConverter
{
    LuminaireDto Convert(Luminaire luminaire);
}