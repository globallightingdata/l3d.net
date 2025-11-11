using L3D.Net.XML.V0_11_0.Dto;
using System;
using System.IO;
using System.Xml.Serialization;
using L3D.Net.Abstract;

namespace L3D.Net.XML.V0_11_0;

internal class XmlDtoSerializer : IXmlDtoSerializer
{
    private readonly XmlSerializer _serializer;
    private readonly XmlSerializerNamespaces _namespaces = new();

    internal XmlDtoSerializer()
    {
        _serializer = new XmlSerializer(typeof(LuminaireDto));
        _namespaces.Add("", "");
        _namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
    }

    public void Serialize(LuminaireDto dto, Stream stream)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        dto.Scheme = Constants.CurrentSchemeUri;

        _serializer.Serialize(stream, dto, _namespaces);
    }

    public LuminaireDto Deserialize(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        return (LuminaireDto) _serializer.Deserialize(stream)!;
    }
}