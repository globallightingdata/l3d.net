namespace L3D.Net.Internal.Abstract;

public interface IContainerDirectory
{
    string Path { get; }

    void CleanUp();
}