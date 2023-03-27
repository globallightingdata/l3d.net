using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using System;
using System.IO;

namespace L3D.Net.Internal;

public class ContainerReader : IContainerReader
{
    private readonly IFileHandler _fileHandler;
    private readonly IL3DXmlReader _l3DXmlReader;

    public ContainerReader(IFileHandler fileHandler, IL3DXmlReader il3DXmlReader)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _l3DXmlReader = il3DXmlReader ?? throw new ArgumentNullException(nameof(il3DXmlReader));
    }

    public Luminaire Read(string containerPath)
    {
        if (string.IsNullOrWhiteSpace(containerPath))
            throw new ArgumentException(@"Value cannot be null or whitespace", nameof(containerPath));
        return ReadInternal(directory => _fileHandler.ExtractContainerToDirectory(containerPath, directory));
    }

    public Luminaire Read(byte[] containerBytes)
    {
        if (containerBytes == null || containerBytes.LongLength == 0)
            throw new ArgumentException(@"Value cannot be null or empty array", nameof(containerBytes));
        return ReadInternal(directory => _fileHandler.ExtractContainerToDirectory(containerBytes, directory));
    }

    public Luminaire Read(Stream containerStream)
    {
        if (containerStream == null || containerStream.Length == 0)
            throw new ArgumentException(@"Value cannot be null or empty array", nameof(containerStream));
        return ReadInternal(directory => _fileHandler.ExtractContainerToDirectory(containerStream, directory));
    }

    private Luminaire ReadInternal(Action<string> extractAction)
    {
        using var directoryScope = new ContainerDirectoryScope(_fileHandler.CreateContainerDirectory());
        extractAction(directoryScope.Directory);
        var structurePath = Path.Combine(directoryScope.Directory, Constants.L3dXmlFilename);
        return _l3DXmlReader.Read(structurePath, directoryScope.Directory);
    }
}