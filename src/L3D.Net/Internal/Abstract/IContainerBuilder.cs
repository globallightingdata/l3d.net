using L3D.Net.Data;
using System.IO;

namespace L3D.Net.Internal.Abstract;

public interface IContainerBuilder
{
    void CreateContainerFile(Luminaire luminaire, string containerPath);
    byte[] CreateContainerByteArray(Luminaire luminaire);
    void AppendContainerToStream(Luminaire luminaire, Stream stream);
}