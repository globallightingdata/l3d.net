using Microsoft.Extensions.Logging;

namespace L3D.Net.Abstract;

public interface IValidator
{
    bool ValidateContainer(string containerPath, ILogger logger = null);

    bool ValidateContainer(byte[] containerBytes, ILogger logger = null);
}