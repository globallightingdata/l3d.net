using L3D.Net.XML.V0_9_0.Dto;

namespace L3D.Net.XML.V0_9_0
{
    interface IXmlDtoSerializer
    {
        void Serialize(LuminaireDto dto, string filename);
        LuminaireDto Deserialize(string filename);
    }
}
