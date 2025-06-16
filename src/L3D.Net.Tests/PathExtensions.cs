using L3D.Net.Abstract;
using L3D.Net.Internal.Abstract;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace L3D.Net.Tests;

public static class PathExtensions
{
    public static List<Stream> Streams { get; } = new();

    public static ContainerCache ToCache(this string directory)
        => ToCache(directory, Constants.L3dXmlFilename);

    public static ContainerCache ToCache(this string directory, string xmlName)
        => ToCache(new DirectoryInfo(directory), xmlName);

    public static ContainerCache ToCache(this DirectoryInfo directory)
        => ToCache(directory, Constants.L3dXmlFilename);

    public static ContainerCache ToCache(this DirectoryInfo directory, string xmlName)
    {
        var structureXml = Path.Combine(directory.FullName, xmlName);
        var structureStream = Stream.Null;
        if (File.Exists(structureXml))
        {
            structureStream = File.OpenRead(structureXml);
            Streams.Add(structureStream);
        }

        return new ContainerCache
        {
            StructureXml = structureStream,
            Geometries = directory.GetDirectories().ToDictionary(d => d.Name, y =>
            {
                var geometries = new Dictionary<string, Stream>();
                foreach (var file in y.GetFiles())
                {
                    var ms = new MemoryStream();
                    using var fs = file.OpenRead();
                    fs.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    Streams.Add(ms);
                    geometries.Add(file.Name, ms);
                }

                return geometries;
            })
        };
    }
}