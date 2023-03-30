using System.IO;
using L3D.Net.XML.V0_10_0.Dto;

namespace L3D.Net.XML.V0_10_0;

internal interface IXmlDtoSerializer
{
    void Serialize(LuminaireDto dto, Stream stream);
    LuminaireDto Deserialize(Stream stream);
}