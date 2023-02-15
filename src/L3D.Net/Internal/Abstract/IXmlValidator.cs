using Microsoft.Extensions.Logging;

namespace L3D.Net.Internal.Abstract;

interface IXmlValidator
{
    bool ValidateFile(string xmlFilename, ILogger logger);
}