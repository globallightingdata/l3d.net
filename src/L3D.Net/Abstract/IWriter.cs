using L3D.Net.Data;
using Microsoft.Extensions.Logging;
using System.IO;

namespace L3D.Net.Abstract
{
    public interface IWriter
    {
        void WriteToFile(Luminaire luminaire, string containerPath, ILogger? logger = null);

        byte[] WriteToByteArray(Luminaire luminaire, ILogger? logger = null);

        void WriteToStream(Luminaire luminaire, Stream containerStream, ILogger? logger = null);
    }
}
