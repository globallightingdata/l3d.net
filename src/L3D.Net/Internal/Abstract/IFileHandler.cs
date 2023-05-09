using System.IO;

namespace L3D.Net.Internal.Abstract;

internal interface IFileHandler
{
    void CreateContainerFile(ContainerCache cache, string containerPath);
    byte[] CreateContainerByteArray(ContainerCache cache);
    void AppendContainerToStream(ContainerCache cache, Stream stream);
    void AddModelFilesToCache(IModel3D model3D, string geometryId, ContainerCache cache);
    ContainerCache ExtractContainerOrThrow(string containerPath);
    ContainerCache ExtractContainerOrThrow(byte[] containerBytes);
    ContainerCache ExtractContainerOrThrow(Stream containerStream);
    ContainerCache? ExtractContainer(string containerPath);
    ContainerCache? ExtractContainer(byte[] containerBytes);
    ContainerCache? ExtractContainer(Stream containerStream);
    byte[] GetTextureBytes(ContainerCache cache, string geomId, string textureName);
}