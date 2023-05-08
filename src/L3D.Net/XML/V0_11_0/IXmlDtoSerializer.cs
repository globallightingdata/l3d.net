using L3D.Net.XML.V0_11_0.Dto;
using System.IO;

namespace L3D.Net.XML.V0_11_0;

internal interface IXmlDtoSerializer
{
    void Serialize(LuminaireDto dto, Stream stream);
    LuminaireDto Deserialize(Stream stream);
}