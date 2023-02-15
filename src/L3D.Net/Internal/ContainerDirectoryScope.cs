using System;
using L3D.Net.Internal.Abstract;

namespace L3D.Net.Internal;

internal sealed class ContainerDirectoryScope : IDisposable
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