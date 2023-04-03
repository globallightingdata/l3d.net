using L3D.Net.Data;
using System.IO;

namespace L3D.Net.Abstract;

internal interface IContainerReader
{
    Luminaire Read(string containerPath);

    Luminaire Read(byte[] containerBytes);

    Luminaire Read(Stream containerStream);
}