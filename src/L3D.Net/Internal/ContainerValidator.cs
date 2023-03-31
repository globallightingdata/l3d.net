using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace L3D.Net.Internal;

internal class ContainerValidator : IContainerValidator
{
    private readonly IFileHandler _fileHandler;
    private readonly IXmlValidator _xmlValidator;
    private readonly IL3DXmlReader _l3dXmlReader;

    public ContainerValidator(IFileHandler fileHandler, IXmlValidator xmlValidator, IL3DXmlReader l3DXmlReader)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _xmlValidator = xmlValidator ?? throw new ArgumentNullException(nameof(xmlValidator));
        _l3dXmlReader = l3DXmlReader ?? throw new ArgumentNullException(nameof(l3DXmlReader));
    }

    public IEnumerable<ValidationHint> Validate(string containerPath, Validation flags)
    {
        if (string.IsNullOrWhiteSpace(containerPath))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(containerPath));

        using var cache = _fileHandler.ExtractContainer(containerPath);

        return ValidateCache(cache, flags);
    }

    public IEnumerable<ValidationHint> Validate(byte[] containerBytes, Validation flags)
    {
        if (containerBytes == null || containerBytes.LongLength == 0)
            throw new ArgumentException(@"Value cannot be null or empty array.", nameof(containerBytes));

        using var cache = _fileHandler.ExtractContainer(containerBytes);

        return ValidateCache(cache, flags);
    }

    public IEnumerable<ValidationHint> Validate(Stream containerStream, Validation flags)
    {
        if (containerStream == null || containerStream.Length == 0)
            throw new ArgumentException(@"Value cannot be null or empty array.", nameof(containerStream));

        using var cache = _fileHandler.ExtractContainer(containerStream);

        return ValidateCache(cache, flags);
    }

    private IEnumerable<ValidationHint> ValidateCache(ContainerCache? cache, Validation flags)
    {
        if (cache == null)
        {
            if (flags.HasFlag(Validation.IsZipPackage))
            {
                yield return new InvalidZipValidationHint();
            }
            yield break;
        }

        if (cache.StructureXml == null)
        {
            if (flags.HasFlag(Validation.HasStructureXml))
            {
                yield return new StructureXmlMissingValidationHint();
            }
            yield break;
        }

        var xsdValidationHints = Array.Empty<ValidationHint>();

        if (flags.HasFlag(Validation.IsXmlValid))
        {
            xsdValidationHints = _xmlValidator.ValidateStream(cache.StructureXml).ToArray();

            foreach (var validationHint in xsdValidationHints)
            {
                yield return validationHint;
            }
        }

        if (xsdValidationHints.All(d => d.Severity != Severity.Error))
        {
            if (flags.HasFlag(Validation.IsProductValid))
            {
                var luminaire = _l3dXmlReader.Read(cache);

                if (luminaire == null)
                {
                    yield return new NotAL3DValidationHint();
                    yield break;
                }

                var geometryParts = luminaire.Parts.SelectMany(GetParts);

                foreach (var geometryPart in geometryParts)
                {
                    if (geometryPart.GeometryReference?.Model == null)
                        yield return new MissingGeometryReferenceValidationHint(geometryPart.GeometryReference?.GeometryId ?? "<null>");
                }
            }
            yield break;
        }
    }

    private static IEnumerable<GeometryPart> GetParts(GeometryPart geometryPart)
    {
        yield return geometryPart;

        foreach (var subGeometryPart in geometryPart.Joints.SelectMany(x => x.Geometries))
        {
            foreach (var part in GetParts(subGeometryPart))
            {
                yield return part;
            }
        }
    }
}