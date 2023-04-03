using L3D.Net.Data;
using System.IO;

namespace L3D.Net.Abstract
{
    public interface IWriter
    {
        void WriteToFile(Luminaire luminaire, string containerPath);

        byte[] WriteToByteArray(Luminaire luminaire);

        void WriteToStream(Luminaire luminaire, Stream containerStream);
    }
}
