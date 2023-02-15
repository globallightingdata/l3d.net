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

        switch (version)
        {
            case L3dXmlVersion.V0_9_2: return ReadV0_9_2(filename, workingDirectory);
        }
            
        throw new Exception($"Unknown version of the l3d xml: '{version}'");
    }
        
    private L3dXmlVersion GetVersion(string filepath)
    {
        var xmlDocument = XDocument.Load(filepath);
        var root = xmlDocument.Root ?? throw new Exception($"Unable to read XML content of {filepath}!");
            
        var schemeAttribute = root.Attributes().FirstOrDefault(attribute =>
            attribute.Name is { NamespaceName: @"http://www.w3.org/2001/XMLSchema-instance", LocalName: @"noNamespaceSchemaLocation" });

        if (schemeAttribute == null)
            throw new Exception(
                "XML document does not reference a valid XSD scheme in namespace (http://www.w3.org/2001/XMLSchema-instance)!");

        if (!GlobalXmlDefinitions.SchemeVersions.TryGetValue(schemeAttribute.Value, out var version))
            throw new Exception(
                $"The scheme ({schemeAttribute.Value}) is not known! Try update the L3D.Net component and try again.");

        return version;
    }

    private Luminaire ReadV0_9_2(string filepath, string workingDirectory)
    {
        var xmlSerializer = new XmlDtoSerializer();
        var luminaireConstructor = new LuminaireFromDtoConstructor();
        var luminaireDto = xmlSerializer.Deserialize(filepath);
        var builder = Builder.NewLuminaire(_logger);
        var luminaire = luminaireConstructor.BuildLuminaireFromDto(builder, luminaireDto, workingDirectory);
        return luminaire;
    }
}