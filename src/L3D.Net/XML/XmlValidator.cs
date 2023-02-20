using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net.XML;

public class XmlValidator : IXmlValidator
{
    public bool ValidateFile(string xmlFilename, ILogger validationLogger)
    {
        var xmlDocument = XDocument.Load(xmlFilename);
        var root = xmlDocument.Root;

        if (root == null)
        {
            validationLogger?.LogError("Unable to read XML content of {XmlFilename}!", xmlFilename);
            return false;
        }

        if (!TryGetVersion(root, validationLogger, out var version))
            return false;

        if (!TryLoadXsd(version, validationLogger, out var scheme))
            return false;
        
        var schemeSet = CreateSchemaSet(scheme);
            
        return Validate(xmlDocument, schemeSet, validationLogger);
    }

    private bool Validate(XDocument xmlDocument, XmlSchemaSet schemeSet, ILogger validationLogger)
    {
        var isValid = true;

        try
        {
            xmlDocument.Validate(schemeSet, (_, ev) =>
            {
                if (ev.Severity == XmlSeverityType.Error)
                {
                    isValid = false;
                    validationLogger?.Log(LogLevel.Error, ev.Message);
                }
                else
                {
                    validationLogger?.Log(LogLevel.Warning, ev.Message);
                }
            });
        }
        catch (Exception e)
        {
            isValid = false;
            validationLogger?.Log(LogLevel.Error, e.Message);
        }

        return isValid;
    }

    private XmlSchemaSet CreateSchemaSet(string xsdText)
    {
        using var xsdStringReader = new StringReader(xsdText);
        using var schemaDoc = XmlReader.Create(xsdStringReader);
        var schemaSet = new XmlSchemaSet();
        schemaSet.Add(string.Empty, schemaDoc);
        return schemaSet;
    }

    private bool TryGetVersion(XElement root, ILogger validationLogger, out Version version)
    {
        version = null;

        var schemeAttribute = root.Attributes().FirstOrDefault(attribute =>
            attribute.Name is { NamespaceName: @"http://www.w3.org/2001/XMLSchema-instance", LocalName: @"noNamespaceSchemaLocation" });

        if (schemeAttribute == null)
        {
            validationLogger?.LogError("XML document does not reference a valid XSD scheme in namespace (http://www.w3.org/2001/XMLSchema-instance)!");
            return false;
        }

        var match = GlobalXmlDefinitions.VersionRegex.Match(schemeAttribute.Value);

        if (!match.Success || !Version.TryParse(match.Groups[1].Value, out version) ||
            !GlobalXmlDefinitions.IsParseable(version))
        {
            validationLogger?.LogError("The scheme ({SchemeAttribute}) is not known! Try update the L3D.Net component and try again", schemeAttribute.Value); //ToDo:
            return false;
        }

        version = GlobalXmlDefinitions.GetNextMatchingVersion(version);

        return true;
    }

    private bool TryLoadXsd(Version version, ILogger validationLogger, out string content)
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
            validationLogger?.LogError(e, "Failed to get embedded XSD for version {Version}!", version);
            content = null;
            return false;
        }
    }
}