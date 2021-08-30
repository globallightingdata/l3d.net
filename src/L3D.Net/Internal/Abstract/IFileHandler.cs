namespace L3D.Net.Internal.Abstract
{
    interface IFileHandler
    {
        void CreateContainerFromDirectory(string directory, string containerPath);
        IContainerDirectory CreateContainerDirectory();
        void CopyModelFiles(IModel3D model3D, string targetDirectory);
        void ExtractContainerToDirectory(string containerPath, string directory);
        byte[] GetTextureBytes(string directory, string geomId, string textureName);
    }
}
