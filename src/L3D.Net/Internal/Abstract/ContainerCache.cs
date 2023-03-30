using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace L3D.Net.Internal.Abstract
{
    public class ContainerCache : IDisposable
    {
        private bool _disposed;

        public Stream? StructureXml { get; set; }
        public Dictionary<string, Dictionary<string, Stream>> Geometries { get; set; } = new();

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                StructureXml?.Dispose();

                foreach (var stream in Geometries.Values.SelectMany(x => x.Values))
                {
                    stream?.Dispose();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
