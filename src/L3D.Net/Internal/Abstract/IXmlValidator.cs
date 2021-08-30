using L3D.Net.XML;
using Microsoft.Extensions.Logging;

namespace L3D.Net.Internal.Abstract
{
    interface IXmlValidator
    {
        bool ValidateFile(string xmlFilename, out L3dXmlVersion validatedVersion, ILogger logger);
    }
}
