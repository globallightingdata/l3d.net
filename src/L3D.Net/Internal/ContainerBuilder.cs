using L3D.Net.Data;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_9_2;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace L3D.Net.Internal;

public class ContainerBuilder : IContainerBuilder
{
    private readonly IFileHandler _fileHandler;
    private readonly IXmlDtoConverter _converter;
    private readonly IXmlDtoSerializer _serializer;
    private readonly IXmlValidator _validator;
    private readonly ILogger _logger;

    public ContainerBuilder(IFileHandler fileHandler, IXmlDtoConverter converter, IXmlDtoSerializer serializer, IXmlValidator validator, ILogger logger)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger;
    }

    public byte[] CreateContainerByteArray(Luminaire luminaire)
    {
        if (luminaire == null) throw new ArgumentNullException(nameof(luminaire));

        using var scope = new ContainerDirectoryScope(_fileHandler.CreateContainerDirectory());
        var directory = scope.Directory;
        PrepareFiles(luminaire, directory);
        return _fileHandler.CreateContainerByteArray(directory);
    }

    public void AppendContainerToStream(Luminaire luminaire, Stream stream)
    {
        if (luminaire == null) throw new ArgumentNullException(nameof(luminaire));

        using var scope = new ContainerDirectoryScope(_fileHandler.CreateContainerDirectory());
        var directory = scope.Directory;
        PrepareFiles(luminaire, directory);
        _fileHandler.AppendContainerToStream(directory, stream);
    }

    public void CreateContainerFile(Luminaire luminaire, string containerPath)
    {
        if (luminaire == null) throw new ArgumentNullException(nameof(luminaire));

        if (string.IsNullOrWhiteSpace(containerPath))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(containerPath));

        using var scope = new ContainerDirectoryScope(_fileHandler.CreateContainerDirectory());
        var directory = scope.Directory;
        PrepareFiles(luminaire, directory);
        _fileHandler.CreateContainerFile(directory, containerPath);
    }

    private void PrepareFiles(Luminaire luminaire, string targetDirectory)
    {
        foreach (var geometryDefinition in luminaire.GeometryDefinitions)
        {
            var modelTargetDirectory = Path.Combine(targetDirectory, geometryDefinition.GeometryId);

            _fileHandler.CopyModelFiles(geometryDefinition.Model, modelTargetDirectory);
        }

        var dto = _converter.Convert(luminaire);

        var xmlFilename = Path.Combine(targetDirectory, Constants.L3dXmlFilename);
        _serializer.Serialize(dto, xmlFilename);

        if (!_validator.ValidateFile(xmlFilename, _logger))
            throw new InvalidL3DException("Failed to validate created xml file: " + xmlFilename);
    }
}