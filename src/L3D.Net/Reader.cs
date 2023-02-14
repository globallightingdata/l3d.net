using L3D.Net.Abstract;
using L3D.Net.API;
using L3D.Net.API.Dto;
using L3D.Net.Internal;
using L3D.Net.XML;
using Microsoft.Extensions.Logging;

namespace L3D.Net;

public class Reader : IReader
{
    public LuminaireDto ReadContainer(string containerPath, ILogger logger = null) =>
        CreateContainerReader(logger).Read(containerPath);

    public LuminaireDto ReadContainer(byte[] containerBytes, ILogger logger = null) =>
        CreateContainerReader(logger).Read(containerBytes);

    private static ContainerReader CreateContainerReader(ILogger logger)
    {
        var fileHandler = new FileHandler();
        return new ContainerReader(fileHandler,
            new L3dXmlReader(new XmlValidator(), logger),
            new ApiDtoConverter(fileHandler));
    }
}