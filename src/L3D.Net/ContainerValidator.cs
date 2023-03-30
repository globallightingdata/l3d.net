using L3D.Net.Abstract;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace L3D.Net;

public class ContainerValidator : IContainerValidator
{
    private readonly IFileHandler _fileHandler;
    private readonly IXmlValidator _xmlValidator;
    private readonly ILogger? _logger;

    public ContainerValidator(IFileHandler fileHandler, IXmlValidator xmlValidator, ILogger? logger)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _xmlValidator = xmlValidator ?? throw new ArgumentNullException(nameof(xmlValidator));
        _logger = logger;
    }

    public bool Validate(string containerPath)
    {
        if (string.IsNullOrWhiteSpace(containerPath))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(containerPath));

        using var cache = _fileHandler.ExtractContainer(containerPath);
        return _xmlValidator.ValidateStream(cache.StructureXml!, _logger);
    }

    public bool Validate(byte[] containerBytes)
    {
        if (containerBytes == null || containerBytes.LongLength == 0)
            throw new ArgumentException(@"Value cannot be null or empty array.", nameof(containerBytes));

        using var cache = _fileHandler.ExtractContainer(containerBytes);
        return _xmlValidator.ValidateStream(cache.StructureXml!, _logger);
    }

    public bool Validate(Stream containerStream)
    {
        if (containerStream == null || containerStream.Length == 0)
            throw new ArgumentException(@"Value cannot be null or empty array.", nameof(containerStream));

        using var cache = _fileHandler.ExtractContainer(containerStream);
        return _xmlValidator.ValidateStream(cache.StructureXml!, _logger);
    }
}