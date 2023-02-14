using System;
using System.IO;
using L3D.Net.Abstract;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net;

class ContainerValidator : IContainerValidator
{
    private readonly IFileHandler _fileHandler;
    private readonly IXmlValidator _xmlValidator;
    private readonly ILogger _logger;

    public ContainerValidator(IFileHandler fileHandler, IXmlValidator xmlValidator, ILogger logger)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _xmlValidator = xmlValidator ?? throw new ArgumentNullException(nameof(xmlValidator));
        _logger = logger;
    }

    public bool Validate(string containerPath)
    {
        if (string.IsNullOrWhiteSpace(containerPath))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(containerPath));

        using var directoryScope = new ContainerDirectoryScope(_fileHandler.CreateContainerDirectory());
        _fileHandler.ExtractContainerToDirectory(containerPath, directoryScope.Directory);
        var structurePath = Path.Combine(directoryScope.Directory, Constants.L3dXmlFilename);
        return _xmlValidator.ValidateFile(structurePath, _logger);
    }

    public bool Validate(byte[] containerBytes)
    {
        if (containerBytes == null || containerBytes.LongLength == 0)
            throw new ArgumentException(@"Value cannot be null or empty array.", nameof(containerBytes));

        using var directoryScope = new ContainerDirectoryScope(_fileHandler.CreateContainerDirectory());
        _fileHandler.ExtractContainerToDirectory(containerBytes, directoryScope.Directory);
        var structurePath = Path.Combine(directoryScope.Directory, Constants.L3dXmlFilename);
        return _xmlValidator.ValidateFile(structurePath, _logger);
    }
}