using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace L3D.Net.Internal.Abstract;

public interface IObjParser
{
    IModel3D? Parse(string fileName, Dictionary<string, Stream> files, ILogger? logger = null);
}