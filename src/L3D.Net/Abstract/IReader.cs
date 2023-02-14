using L3D.Net.API.Dto;
using Microsoft.Extensions.Logging;

namespace L3D.Net.Abstract
{
    public interface IReader
    {
        public LuminaireDto ReadContainer(string containerPath, ILogger logger = null);

        public LuminaireDto ReadContainer(byte[] containerBytes, ILogger logger = null);
    }
}