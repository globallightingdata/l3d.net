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
        var root = xmlDocument.Root ?? throw new Exception($"Unable to read XML content of {xmlFilename}!");

        var scheme = LoadXsd(GetVersion(root));
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
                    validationLogger?.Log(LogLevel.Error ,ev.Message);
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

    private L3dXmlVersion GetVersion(XElement root)
    {
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

    private string LoadXsd(L3dXmlVersion version)
    {
        try
        {
            var xsdResourceName = $"L3D.Net.XSD.{version.ToString()}.xsd";
            var currentAssembly = Assembly.GetAssembly(typeof(XmlValidator));
            using var xsdResource = currentAssembly.GetManifestResourceStream(xsdResourceName);
            using var reader = new StreamReader(xsdResource!, Encoding.UTF8);
            return reader.ReadToEnd();
        }
        catch (Exception e)
        {
            var errorMessage = $"Failed to get embedded XSD for version {version}!";
            throw new Exception(errorMessage, e);
        }
    }
}