using L3D.Net.Internal.Abstract;
using System;
using System.IO;

namespace L3D.Net.Internal;

internal class ContainerDirectory : IContainerDirectory
{
    public string Path { get; }

    public ContainerDirectory()
    {
        while (Path == null || Directory.Exists(Path))
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Constants.TempWorkingSubDirectory, Guid.NewGuid().ToString());
        }

        Directory.CreateDirectory(Path);
    }

    public void CleanUp()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}