using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Internal.Abstract;

public interface IObjParser
{
    public IModel3D? Parse(string filePath, ILogger? logger = null);

    public IModel3D? Parse(string fileName, Dictionary<string, Stream> files, ILogger? logger = null);
}