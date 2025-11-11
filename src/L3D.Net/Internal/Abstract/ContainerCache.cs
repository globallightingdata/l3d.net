using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace L3D.Net.Internal.Abstract;

public sealed class ContainerCache : IDisposable
{
    private bool _disposed;

    public Stream? StructureXml { get; set; }

    public Dictionary<string, Dictionary<string, Stream>> Geometries { get; set; } = new();

    public void Dispose()
    {
        if (_disposed) return;
        StructureXml?.Dispose();
        foreach (var stream in Geometries.Values.SelectMany(x => x.Values))
        {
            stream.Dispose();
        }

        _disposed = true;
    }
}