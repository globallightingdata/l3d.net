using L3D.Net.Data;
using System.IO;

namespace L3D.Net.Abstract;

internal interface IContainerReader
{
    public Luminaire Read(string containerPath);

    public Luminaire Read(byte[] containerBytes);

    public Luminaire Read(Stream containerStream);
}