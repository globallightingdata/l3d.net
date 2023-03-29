using Microsoft.Extensions.Logging;

namespace L3D.Net.Internal.Abstract;

public interface IXmlValidator
{
    bool ValidateFile(string xmlFilename, ILogger? logger = null);
}