using L3D.Net.Data;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using L3D.Net.Mapper.V0_9_2;
using L3D.Net.XML.V0_9_2;
using System;
using System.Linq;
using System.Xml.Linq;

namespace L3D.Net.XML;

public class L3DXmlReader : IL3DXmlReader
{
    private readonly XmlDtoSerializer _serializer;

    public L3DXmlReader()
    {
        _serializer = new XmlDtoSerializer();
    }

    public Luminaire Read(string filename, string workingDirectory)
    {
        var version = GetVersion(filename);

        if (version.Major == 0)
            return ReadV0(filename, workingDirectory);

        throw new InvalidL3DException($"Unknown version of the l3d xml: '{version}'");
    }

    private static Version GetVersion(string filepath)
    {
        var xmlDocument = XDocument.Load(filepath);
        var root = xmlDocument.Root ?? throw new InvalidL3DException($"Unable to read XML content of {filepath}!");

        var schemeAttribute = root.Attributes().FirstOrDefault(attribute =>
            attribute.Name is { NamespaceName: @"http://www.w3.org/2001/XMLSchema-instance", LocalName: @"noNamespaceSchemaLocation" }) ?? throw new InvalidL3DException(
                "XML document does not reference a valid XSD scheme in namespace (http://www.w3.org/2001/XMLSchema-instance)!");
        var match = GlobalXmlDefinitions.VersionRegex.Match(schemeAttribute.Value);

        if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var version) || !GlobalXmlDefinitions.IsParseable(version))
            throw new InvalidL3DException(
                $"The scheme ({schemeAttribute.Value}) is not known!");

        version = GlobalXmlDefinitions.GetNextMatchingVersion(version);

        return version;
    }

    private Luminaire ReadV0(string filepath, string workingDirectory)
    {
        var luminaireDto = _serializer.Deserialize(filepath);
        var luminaire = LuminaireMapper.Instance.Convert(luminaireDto);

        luminaire = LuminaireResolver.Instance.Resolve(luminaire, workingDirectory);

        return luminaire;
    }
}