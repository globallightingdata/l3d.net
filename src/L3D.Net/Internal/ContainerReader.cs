using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using System;
using System.IO;

namespace L3D.Net.Internal;

internal class ContainerReader : IContainerReader
{
    private readonly IFileHandler _fileHandler;
    private readonly IL3DXmlReader _l3DXmlReader;

    internal ContainerReader(IFileHandler fileHandler, IL3DXmlReader il3DXmlReader)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _l3DXmlReader = il3DXmlReader ?? throw new ArgumentNullException(nameof(il3DXmlReader));
    }

    public Luminaire Read(string containerPath)
    {
        if (string.IsNullOrWhiteSpace(containerPath))
            throw new ArgumentException(@"Value cannot be null or whitespace", nameof(containerPath));
        return ReadInternal(() => _fileHandler.ExtractContainer(containerPath));
    }

    public Luminaire Read(byte[] containerBytes)
    {
        if (containerBytes == null || containerBytes.LongLength == 0)
            throw new ArgumentException(@"Value cannot be null or empty array", nameof(containerBytes));
        return ReadInternal(() => _fileHandler.ExtractContainer(containerBytes));
    }

    public Luminaire Read(Stream containerStream)
    {
        if (containerStream == null || containerStream.Length == 0)
            throw new ArgumentException(@"Value cannot be null or empty array", nameof(containerStream));
        return ReadInternal(() => _fileHandler.ExtractContainer(containerStream));
    }

    private Luminaire ReadInternal(Func<ContainerCache> extractAction)
    {
        using var cache = extractAction();
        return _l3DXmlReader.Read(cache);
    }
}