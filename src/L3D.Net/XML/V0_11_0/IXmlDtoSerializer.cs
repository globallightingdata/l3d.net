using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.XML.V0_11_0;

internal interface IXmlDtoSerializer
{
    void Serialize(LuminaireDto dto, string filename);
    LuminaireDto Deserialize(string filename);
}