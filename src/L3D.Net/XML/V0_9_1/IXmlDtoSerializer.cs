using L3D.Net.XML.V0_9_1.Dto;

namespace L3D.Net.XML.V0_9_1
{
    interface IXmlDtoSerializer
    {
        void Serialize(LuminaireDto dto, string filename);
        LuminaireDto Deserialize(string filename);
    }
}
