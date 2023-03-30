using System.IO;

namespace L3D.Net.Internal.Abstract;

public interface IFileHandler
{
    void CreateContainerFile(ContainerCache cache, string containerPath);
    byte[] CreateContainerByteArray(ContainerCache cache);
    void AppendContainerToStream(ContainerCache cache, Stream stream);
    void LoadModelFiles(IModel3D model3D, string geometryId, ContainerCache cache);
    ContainerCache ExtractContainer(string containerPath);
    ContainerCache ExtractContainer(byte[] containerBytes);
    ContainerCache ExtractContainer(Stream containerStream);
    byte[] GetTextureBytes(ContainerCache cache, string geomId, string textureName);
}