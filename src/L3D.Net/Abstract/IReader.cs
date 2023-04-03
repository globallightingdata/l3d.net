using L3D.Net.Data;
using System.IO;

namespace L3D.Net.Abstract;

public interface IReader
{
    Luminaire ReadContainer(string containerPath);

    Luminaire ReadContainer(byte[] containerBytes);

    Luminaire ReadContainer(Stream containerStream);
}