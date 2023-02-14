using L3D.Net.API.Dto;
using Microsoft.Extensions.Logging;

namespace L3D.Net.Abstract;

public interface IReader
{
    LuminaireDto ReadContainer(string containerPath, ILogger logger = null);

    LuminaireDto ReadContainer(byte[] containerBytes, ILogger logger = null);
}