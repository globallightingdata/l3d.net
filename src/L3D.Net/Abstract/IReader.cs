using L3D.Net.Data;
using System.IO;

namespace L3D.Net.Abstract;

public interface IReader
{
    public Luminaire ReadContainer(string containerPath);

    public Luminaire ReadContainer(byte[] containerBytes);

    public Luminaire ReadContainer(Stream containerStream);
}