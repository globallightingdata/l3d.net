using System.IO;

namespace L3D.Net.Internal.Abstract;

public interface IFileHandler
{
    void CreateContainerFile(string directory, string containerPath);
    byte[] CreateContainerByteArray(string directory);
    void AppendContainerToStream(string directory, Stream stream);
    IContainerDirectory CreateContainerDirectory();
    void CopyModelFiles(IModel3D model3D, string targetDirectory);
    void ExtractContainerToDirectory(string containerPath, string directory);
    void ExtractContainerToDirectory(byte[] containerBytes, string directory);
    void ExtractContainerToDirectory(Stream containerStream, string directory);
    byte[] GetTextureBytes(string directory, string geomId, string textureName);
}