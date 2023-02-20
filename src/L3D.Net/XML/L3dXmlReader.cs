using System;
using System.Linq;
using System.Xml.Linq;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_9_2;
using Microsoft.Extensions.Logging;

namespace L3D.Net.XML;

internal class L3dXmlReader : IL3dXmlReader
{
    private readonly ILogger _logger;

    public L3dXmlReader(ILogger logger)
    {
        _logger = logger;
    }

    public Luminaire Read(string filename, string workingDirectory)
    {
        var version = GetVersion(filename);

        if (version.Major == 0) 
            return ReadV0(filename, workingDirectory);

        throw new Exception($"Unknown version of the l3d xml: '{version}'");
    }
        
    private Version GetVersion(string filepath)
    {
        var xmlDocument = XDocument.Load(filepath);
        var root = xmlDocument.Root ?? throw new Exception($"Unable to read XML content of {filepath}!");
            
        var schemeAttribute = root.Attributes().FirstOrDefault(attribute =>
            attribute.Name is { NamespaceName: @"http://www.w3.org/2001/XMLSchema-instance", LocalName: @"noNamespaceSchemaLocation" });

        if (schemeAttribute == null)
            throw new Exception(
                "XML document does not reference a valid XSD scheme in namespace (http://www.w3.org/2001/XMLSchema-instance)!");

        var match = GlobalXmlDefinitions.VersionRegex.Match(schemeAttribute.Value);

        if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var version) || !GlobalXmlDefinitions.IsParseable(version))
            throw new Exception(
                $"The scheme ({schemeAttribute.Value}) is not known! Try update the L3D.Net component and try again."); //ToDo:

        version = GlobalXmlDefinitions.GetNextMatchingVersion(version);

        return version;
    }

    private Luminaire ReadV0(string filepath, string workingDirectory)
    {
        var xmlSerializer = new XmlDtoSerializer();
        var luminaireConstructor = new LuminaireFromDtoConstructor();
        var luminaireDto = xmlSerializer.Deserialize(filepath);
        var builder = Builder.NewLuminaire(_logger);
        var luminaire = luminaireConstructor.BuildLuminaireFromDto(builder, luminaireDto, workingDirectory);
        return luminaire;
    }
}