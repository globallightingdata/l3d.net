using System;
using System.IO;
using System.Xml.Serialization;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.XML.V0_11_0;

internal class XmlDtoSerializer : IXmlDtoSerializer
{
    private readonly XmlSerializer _serializer;

    internal XmlDtoSerializer()
    {
        _serializer = new XmlSerializer(typeof(LuminaireDto));
    }

    public void Serialize(LuminaireDto dto, Stream stream)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

        _serializer.Serialize(stream, dto, ns);
    }

    public LuminaireDto Deserialize(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        return (LuminaireDto)_serializer.Deserialize(stream);
    }
}