using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using L3D.Net.Mapper.V0_11_0;
using System;
using System.IO;
using System.Linq;
using L3D.Net.XML.V0_11_0;

namespace L3D.Net.Internal;

internal class ContainerBuilder : IContainerBuilder
{
    private readonly IFileHandler _fileHandler;
    private readonly IXmlDtoSerializer _serializer;
    private readonly IXmlValidator _validator;

    internal ContainerBuilder(IFileHandler fileHandler, IXmlDtoSerializer serializer, IXmlValidator validator)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
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
            if (geometryDefinition.Model != null)
                _fileHandler.LoadModelFiles(geometryDefinition.Model, geometryDefinition.GeometryId, cache);
        }

        var dto = LuminaireMapper.Instance.Convert(luminaire);

        cache.StructureXml = new MemoryStream();
        _serializer.Serialize(dto, cache.StructureXml);

        cache.StructureXml.Seek(0, SeekOrigin.Begin);

        if (_validator.ValidateStream(cache.StructureXml).Any(x => x.Severity == Severity.Error))
            throw new InvalidL3DException("Failed to validate created xml file");
    }
}