using L3D.Net.Abstract;
using L3D.Net.Internal.Abstract;
using System;
using System.Collections.Generic;
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
    public IEnumerable<ValidationHint> ValidateStream(Stream xmlStream)
    {
        if (!TryLoadDocument(xmlStream, out var document) || document!.Root == null)
        {
            yield return new StructureXmlValidationHint(ErrorMessages.StructureXmlNotReadable);
            yield break;
        }

        if (!TryGetSchemeAttribute(document.Root!, out var attribute))
        {
            yield return new StructureXmlValidationHint(ErrorMessages.StructureXmlVersionMissing);
            yield break;
        }

        if (!TryGetVersion(attribute!, out var version))
        {
            yield return new StructureXmlValidationHint(ErrorMessages.StructureXmlVersionNotReadable, attribute!.Value);
            yield break;
        }

        if (!TryLoadXsd(version!, out var scheme))
        {
            yield return new StructureXmlValidationHint(ErrorMessages.StructureXsdVersionNotKnown, $"Version: {version}");
            yield break;
        }

        var schemeSet = CreateSchemaSet(scheme!);

        foreach (var validationHint in Validate(document, schemeSet))
        {
            yield return validationHint;
        }
    }

    private static bool TryLoadDocument(Stream xmlStream, out XDocument? document)
    {
        try
        {
            document = XDocument.Load(xmlStream);
            return true;
        }
        catch
        {
            document = null;
            return false;
        }
    }

    private static IEnumerable<ValidationHint> Validate(XDocument xmlDocument, XmlSchemaSet schemeSet)
    {
        var validationHints = new List<ValidationHint>();

        try
        {
            xmlDocument.Validate(schemeSet, (_, ev) =>
            {
                if (ev.Severity == XmlSeverityType.Error)
                {
                    validationHints.Add(new StructureXmlValidationHint(ErrorMessages.StructureXmlContentError, ev.Message, Severity.Error));
                }
                else
                {
                    validationHints.Add(new StructureXmlValidationHint(ErrorMessages.StructureXmlContentWarning, ev.Message, Severity.Warning));
                }
            });
        }
        catch (Exception e)
        {
            validationHints.Add(new StructureXmlValidationHint(ErrorMessages.StructureXmlContentValidationFailed, e.Message));
        }

        return validationHints;
    }

    private static XmlSchemaSet CreateSchemaSet(string xsdText)
    {
        using var xsdStringReader = new StringReader(xsdText);
        using var schemaDoc = XmlReader.Create(xsdStringReader);
        var schemaSet = new XmlSchemaSet();
        schemaSet.Add(string.Empty, schemaDoc);
        return schemaSet;
    }

    private static bool TryGetSchemeAttribute(XElement root, out XAttribute? attribute)
    {
        var schemeAttribute = root.Attributes().FirstOrDefault(attribute =>
            attribute.Name is { NamespaceName: @"http://www.w3.org/2001/XMLSchema-instance", LocalName: @"noNamespaceSchemaLocation" });

        if (schemeAttribute == null)
        {
            attribute = null;
            return false;
        }

        attribute = schemeAttribute;
        return true;
    }

    private static bool TryGetVersion(XAttribute attribute, out Version? version)
    {
        var match = GlobalXmlDefinitions.VersionRegex.Match(attribute.Value);

        if (!match.Success || !Version.TryParse(match.Groups[1].Value, out version) ||
            !GlobalXmlDefinitions.IsParseable(version))
        {
            version = null;
            return false;
        }

        version = GlobalXmlDefinitions.GetNextMatchingVersion(version);

        return true;
    }

    private static bool TryLoadXsd(Version version, out string? content)
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
        catch
        {
            content = null;
            return false;
        }
    }
}