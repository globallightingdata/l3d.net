using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace L3D.Net.XML;

public class XmlValidator : IXmlValidator
{
    public bool ValidateStream(Stream xmlStream, ILogger? logger = null)
    {
        var xmlDocument = XDocument.Load(xmlStream);
        var root = xmlDocument.Root;

        if (root == null)
        {
            logger?.LogError("Unable to read XML content");
            return false;
        }

        if (!TryGetVersion(root, logger, out var version))
            return false;

        if (!TryLoadXsd(version!, logger, out var scheme))
            return false;

        var schemeSet = CreateSchemaSet(scheme!);

        return Validate(xmlDocument, schemeSet, logger);
    }

    private static bool Validate(XDocument xmlDocument, XmlSchemaSet schemeSet, ILogger? logger)
    {
        var isValid = true;

        try
        {
            xmlDocument.Validate(schemeSet, (_, ev) =>
            {
                if (ev.Severity == XmlSeverityType.Error)
                {
                    isValid = false;
                    logger?.LogError(ev.Message);
                }
                else
                {
                    logger?.LogWarning(ev.Message);
                }
            });
        }
        catch (Exception e)
        {
            isValid = false;
            logger?.LogError(e, "Could not validate");
        }

        return isValid;
    }

    private static XmlSchemaSet CreateSchemaSet(string xsdText)
    {
        using var xsdStringReader = new StringReader(xsdText);
        using var schemaDoc = XmlReader.Create(xsdStringReader);
        var schemaSet = new XmlSchemaSet();
        schemaSet.Add(string.Empty, schemaDoc);
        return schemaSet;
    }

    private static bool TryGetVersion(XElement root, ILogger? logger, out Version? version)
    {
        version = null;

        var schemeAttribute = root.Attributes().FirstOrDefault(attribute =>
            attribute.Name is { NamespaceName: @"http://www.w3.org/2001/XMLSchema-instance", LocalName: @"noNamespaceSchemaLocation" });

        if (schemeAttribute == null)
        {
            logger?.LogError("XML document does not reference a valid XSD scheme in namespace (http://www.w3.org/2001/XMLSchema-instance)!");
            return false;
        }

        var match = GlobalXmlDefinitions.VersionRegex.Match(schemeAttribute.Value);

        if (!match.Success || !Version.TryParse(match.Groups[1].Value, out version) ||
            !GlobalXmlDefinitions.IsParseable(version))
        {
            logger?.LogError("The scheme ({SchemeAttribute}) is not known!", schemeAttribute.Value);
            return false;
        }

        version = GlobalXmlDefinitions.GetNextMatchingVersion(version);

        return true;
    }

    private static bool TryLoadXsd(Version version, ILogger? logger, out string? content)
    {
        try
        {
            var xsdResourceName = $"L3D.Net.XSD.V{version.Major}_{version.Minor}_{version.Build}.xsd";
            var currentAssembly = Assembly.GetAssembly(typeof(XmlValidator));
            using var xsdResource = currentAssembly.GetManifestResourceStream(xsdResourceName);
            using var reader = new StreamReader(xsdResource!, Encoding.UTF8);
            content = reader.ReadToEnd();
            return true;
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Failed to get embedded XSD for version {Version}!", version);
            content = null;
            return false;
        }
    }
}