using L3D.Net.Abstract;
using L3D.Net.Internal.Abstract;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace L3D.Net.Tests
{
    public static class PathExtensions
    {
        public static readonly List<Stream> Streams = new();

        public static ContainerCache ToCache(this string directory)
        {
            var structure = Path.Combine(directory, Constants.L3dXmlFilename);

            var structureStream = Stream.Null;
            if (File.Exists(structure))
            {
                structureStream = File.OpenRead(structure);
                Streams.Add(structureStream);
            }

            return new()
            {
                StructureXml = structureStream,
                Geometries = Directory.GetDirectories(directory).ToDictionary(d => Path.GetFileName(d)!, y =>
                {
                    var geometries = new Dictionary<string, Stream>();

                    var files = Directory.GetFiles(y);
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);

                        var ms = new MemoryStream();
                        using var fs = File.OpenRead(file);
                        fs.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        Streams.Add(ms);
                        geometries.Add(fileName, ms);
                    }

                    return geometries;
                })!
            };
        }
    }
}
