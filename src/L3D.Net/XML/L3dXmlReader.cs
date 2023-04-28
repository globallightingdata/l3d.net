using L3D.Net.Data;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using L3D.Net.Mapper.V0_11_0;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using L3D.Net.Abstract;
using L3D.Net.XML.V0_11_0;

namespace L3D.Net.XML;

internal class L3DXmlReader : IL3DXmlReader
{
    private readonly XmlDtoSerializer _serializer;

    public L3DXmlReader()
    {
        _serializer = new XmlDtoSerializer();
    }

    public Luminaire? Read(ContainerCache cache)
    {
        if (cache.StructureXml == null)
            throw new ArgumentException($"{nameof(cache.StructureXml)} in {nameof(cache)} cannot be null");

        var version = GetVersion(cache.StructureXml);

        if (version.Major == 0)
            return ReadV0(cache);

        throw new InvalidL3DException($"Unknown version of the l3d xml: '{version}'");
    }

    private static Version GetVersion(Stream stream)
    {
        try
        {
            stream.Seek(0, SeekOrigin.Begin);
            var xmlDocument = XDocument.Load(stream);
            var root = xmlDocument.Root ?? throw new InvalidL3DException($"Unable to read XML content");

            if (root.Attributes().All(attribute => attribute.Name is not
                {
                    NamespaceName: @"http://www.w3.org/2001/XMLSchema-instance", LocalName: @"noNamespaceSchemaLocation"
                }))
                throw new InvalidL3DException(
                    "XML document does not reference a valid XSD scheme in namespace (http://www.w3.org/2001/XMLSchema-instance)!");

            var versionInformation = xmlDocument.XPathSelectElement(Constants.L3dFormatVersionPath)?.Attributes()
                .ToDictionary(d => d.Name.LocalName, d => d.Value);

            if (versionInformation == null ||
                Constants.L3dFormatVersionRequiredFields.Except(versionInformation.Keys).Any() ||
                !TryGetVersion(versionInformation, out var version))
                throw new InvalidL3DException("The version is not known");

            version = GlobalXmlDefinitions.GetNextMatchingVersion(version!);

            return version;
        }
        catch (Exception ex)
        {
            throw new InvalidL3DException("Unable to load version from l3d xml, see inner exception", ex);
        }
    }

    private static bool TryGetVersion(IReadOnlyDictionary<string, string> fields, out Version? version)
    {
        if (!fields.TryGetValue(Constants.L3dFormatVersionMajor, out var majorValue) || !int.TryParse(majorValue, out var major))
        {
            version = null;
            return false;
        }
        if (!fields.TryGetValue(Constants.L3dFormatVersionMinor, out var minorValue) || !int.TryParse(minorValue, out var minor))
        {
            version = null;
            return false;
        }

        var preRelease = 0;

        if (fields.TryGetValue(Constants.L3dFormatVersionPreRelease, out var preReleaseValue) && (!int.TryParse(preReleaseValue, out preRelease) || preRelease < 0))
        {
            version = null;
            return false;
        }

        version = new Version(major, minor, preRelease);
        return true;
    }

    private Luminaire? ReadV0(ContainerCache cache)
    {
        cache.StructureXml!.Seek(0, SeekOrigin.Begin);
        var luminaireDto = _serializer.Deserialize(cache.StructureXml);
        var luminaire = LuminaireMapper.Instance.Convert(luminaireDto);

        luminaire = LuminaireResolver.Instance.Resolve(luminaire, cache);

        return luminaire;
    }
}