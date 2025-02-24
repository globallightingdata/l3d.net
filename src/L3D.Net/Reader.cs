using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.XML;
using System.IO;

namespace L3D.Net;

public class Reader : IReader
{
    public Luminaire ReadContainer(string containerPath) =>
        CreateContainerReader().Read(containerPath);

    public Luminaire ReadContainer(byte[] containerBytes) =>
        CreateContainerReader().Read(containerBytes);

    public Luminaire ReadContainer(Stream containerStream) =>
        CreateContainerReader().Read(containerStream);

    private static IContainerReader CreateContainerReader()
    {
        var fileHandler = new FileHandler();
        return new ContainerReader(fileHandler, new L3DXmlReader());
    }
}