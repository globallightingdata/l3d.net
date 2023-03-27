using L3D.Net.Internal.Abstract;
using System;

namespace L3D.Net.Internal;

public sealed class ContainerDirectoryScope : IDisposable
{
    private readonly IContainerDirectory _directory;

    public string Directory => _directory.Path;

    public ContainerDirectoryScope(IContainerDirectory directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    public void Dispose()
    {
        _directory.CleanUp();
    }
}