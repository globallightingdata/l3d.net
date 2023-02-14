using System;
using System.IO;
using L3D.Net.Abstract;
using L3D.Net.Internal;
using L3D.Net.XML;
using Microsoft.Extensions.Logging;

namespace L3D.Net;

class Validator : IValidator
{
    public bool ValidateContainer(string containerPath, ILogger logger)
    {
        return CreateContainerValidator(logger).Validate(containerPath);
    }

    public bool ValidateContainer(byte[] containerBytes, ILogger logger)
    {
        return CreateContainerValidator(logger).Validate(containerBytes);
    }
    
    private IContainerValidator CreateContainerValidator(ILogger logger)
    {
        var fileHandler = new FileHandler();

        return new ContainerValidator(fileHandler,
                                      new XmlValidator(),
                                      logger);
    }
}