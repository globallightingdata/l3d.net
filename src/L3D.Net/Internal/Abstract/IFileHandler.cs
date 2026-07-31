using System.IO;

namespace L3D.Net.Internal.Abstract;

internal interface IFileHandler
{
    public void CreateContainerFile(ContainerCache cache, string containerPath);

    public byte[] CreateContainerByteArray(ContainerCache cache);

    public void AppendContainerToStream(ContainerCache cache, Stream stream);

    public void AddModelFilesToCache(IModel3D model3D, string geometryId, ContainerCache cache);

    public ContainerCache ExtractContainerOrThrow(string containerPath);

    public ContainerCache ExtractContainerOrThrow(byte[] containerBytes);

    public ContainerCache ExtractContainerOrThrow(Stream containerStream);

    public ContainerCache? ExtractContainer(string containerPath);

    public ContainerCache? ExtractContainer(byte[] containerBytes);

    public ContainerCache? ExtractContainer(Stream containerStream);

    public byte[] GetTextureBytes(ContainerCache cache, string geomId, string textureName);
}