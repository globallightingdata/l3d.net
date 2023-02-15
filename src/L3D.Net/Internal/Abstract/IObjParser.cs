using Microsoft.Extensions.Logging;

namespace L3D.Net.Internal.Abstract;

interface IObjParser
{
    IModel3D Parse(string filePath, ILogger logger);
}