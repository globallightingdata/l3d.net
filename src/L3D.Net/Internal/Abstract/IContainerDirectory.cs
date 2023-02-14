namespace L3D.Net.Internal.Abstract;

interface IContainerDirectory
{
    string Path { get; }

    void CleanUp();
}