using L3D.Net.XML.V0_9_2.Dto;
using System;
using System.IO;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_2;

public class XmlDtoSerializer : IXmlDtoSerializer
{
    public void Serialize(LuminaireDto dto, string filename)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(filename));

        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

        using var stream = File.OpenWrite(filename);
        new XmlSerializer(typeof(LuminaireDto)).Serialize(stream, dto, ns);
    }

    public LuminaireDto Deserialize(string filename)
    {
        using var stream = File.OpenRead(filename);
        return (LuminaireDto)new XmlSerializer(typeof(LuminaireDto)).Deserialize(stream);
    }
}