using L3D.Net.Data;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using L3D.Net.Mapper.V0_11_0;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using L3D.Net.XML.V0_10_0;

namespace L3D.Net.Internal;

internal class ContainerBuilder : IContainerBuilder
{
    private readonly IFileHandler _fileHandler;
    private readonly IXmlDtoSerializer _serializer;
    private readonly IXmlValidator _validator;
    private readonly ILogger? _logger;

    internal ContainerBuilder(IFileHandler fileHandler, IXmlDtoSerializer serializer, IXmlValidator validator, ILogger? logger)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger;
    }

    public byte[] CreateContainerByteArray(Luminaire luminaire)
    {
        if (luminaire == null) throw new ArgumentNullException(nameof(luminaire));

        using var cache = new ContainerCache();

        PrepareFiles(luminaire, cache);
        return _fileHandler.CreateContainerByteArray(cache);
    }

    public void AppendContainerToStream(Luminaire luminaire, Stream stream)
    {
        if (luminaire == null) throw new ArgumentNullException(nameof(luminaire));

        using var cache = new ContainerCache();

        PrepareFiles(luminaire, cache);
        _fileHandler.AppendContainerToStream(cache, stream);
    }

    public void CreateContainerFile(Luminaire luminaire, string containerPath)
    {
        if (luminaire == null) throw new ArgumentNullException(nameof(luminaire));

        if (string.IsNullOrWhiteSpace(containerPath))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(containerPath));

        using var cache = new ContainerCache();

        PrepareFiles(luminaire, cache);
        _fileHandler.CreateContainerFile(cache, containerPath);
    }

    private void PrepareFiles(Luminaire luminaire, ContainerCache cache)
    {
        foreach (var geometryDefinition in luminaire.GeometryDefinitions)
        {
            _fileHandler.LoadModelFiles(geometryDefinition.Model, geometryDefinition.GeometryId, cache);
        }

        var dto = LuminaireMapper.Instance.Convert(luminaire);

        cache.StructureXml = new MemoryStream();
        _serializer.Serialize(dto, cache.StructureXml);

        cache.StructureXml.Seek(0, SeekOrigin.Begin);

        if (!_validator.ValidateStream(cache.StructureXml, _logger))
            throw new InvalidL3DException("Failed to validate created xml file");
    }
}