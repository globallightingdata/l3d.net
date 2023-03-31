using L3D.Net.Abstract;
using L3D.Net.Internal;
using L3D.Net.XML;
using System.Collections.Generic;
using System.IO;

namespace L3D.Net;

public class Validator : IValidator
{
    public IEnumerable<ValidationHint> ValidateContainer(string containerPath, Validation flags)
    {
        return CreateContainerValidator().Validate(containerPath, flags);
    }

    public IEnumerable<ValidationHint> ValidateContainer(byte[] containerBytes, Validation flags)
    {
        return CreateContainerValidator().Validate(containerBytes, flags);
    }

    public IEnumerable<ValidationHint> ValidateContainer(Stream containerStream, Validation flags)
    {
        return CreateContainerValidator().Validate(containerStream, flags);
    }

    private static IContainerValidator CreateContainerValidator()
    {
        var fileHandler = new FileHandler();

        return new ContainerValidator(fileHandler, new XmlValidator(), new L3DXmlReader());
    }
}