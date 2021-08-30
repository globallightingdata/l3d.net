using L3D.Net.API.Dto;

namespace L3D.Net.Abstract
{
    public interface IContainerReader
    {
        LuminaireDto Read(string containerPath);
    }
}
