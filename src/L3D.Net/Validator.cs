using L3D.Net.Abstract;
using L3D.Net.Internal;
using L3D.Net.XML;
using Microsoft.Extensions.Logging;
using System.IO;

namespace L3D.Net;

public class Validator : IValidator
{
    public bool ValidateContainer(string containerPath, ILogger? logger = null)
    {
        return CreateContainerValidator(logger).Validate(containerPath);
    }

    public bool ValidateContainer(byte[] containerBytes, ILogger? logger = null)
    {
        return CreateContainerValidator(logger).Validate(containerBytes);
    }

    public bool ValidateContainer(Stream containerStream, ILogger? logger = null)
    {
        return CreateContainerValidator(logger).Validate(containerStream);
    }

    private static IContainerValidator CreateContainerValidator(ILogger? logger)
    {
        var fileHandler = new FileHandler();

        return new ContainerValidator(fileHandler, new XmlValidator(), logger);
    }
}