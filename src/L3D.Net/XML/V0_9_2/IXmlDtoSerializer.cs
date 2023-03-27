using L3D.Net.XML.V0_9_2.Dto;

namespace L3D.Net.XML.V0_9_2;

public interface IXmlDtoSerializer
{
    void Serialize(LuminaireDto dto, string filename);
    LuminaireDto Deserialize(string filename);
}