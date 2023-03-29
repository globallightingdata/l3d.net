using Microsoft.Extensions.Logging;
using System.IO;

namespace L3D.Net.Abstract;

public interface IValidator
{
    bool ValidateContainer(string containerPath, ILogger? logger = null);

    bool ValidateContainer(byte[] containerBytes, ILogger? logger = null);

    bool ValidateContainer(Stream containerStream, ILogger? logger = null);
}