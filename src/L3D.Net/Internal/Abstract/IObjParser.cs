using Microsoft.Extensions.Logging;

namespace L3D.Net.Internal.Abstract;

public interface IObjParser
{
    IModel3D Parse(string filePath, ILogger logger);
}