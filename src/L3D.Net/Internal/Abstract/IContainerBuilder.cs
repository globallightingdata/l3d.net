using L3D.Net.Data;
using System.IO;

namespace L3D.Net.Internal.Abstract;

internal interface IContainerBuilder
{
    public void CreateContainerFile(Luminaire luminaire, string containerPath);

    public byte[] CreateContainerByteArray(Luminaire luminaire);

    public void AppendContainerToStream(Luminaire luminaire, Stream stream);
}