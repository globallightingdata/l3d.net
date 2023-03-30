using Microsoft.Extensions.Logging;
using System.IO;

namespace L3D.Net.Internal.Abstract;

public interface IXmlValidator
{
    bool ValidateStream(Stream xmlStream, ILogger? logger = null);
}