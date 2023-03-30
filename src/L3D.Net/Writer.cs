using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML;
using Microsoft.Extensions.Logging;
using System.IO;
using L3D.Net.XML.V0_10_0;

namespace L3D.Net
{
    public class Writer : IWriter
    {
        public byte[] WriteToByteArray(Luminaire luminaire, ILogger? logger = null) =>
            CreateContainerWriter(logger).CreateContainerByteArray(luminaire);

        public void WriteToFile(Luminaire luminaire, string containerPath, ILogger? logger = null) =>
            CreateContainerWriter(logger).CreateContainerFile(luminaire, containerPath);

        public void WriteToStream(Luminaire luminaire, Stream containerStream, ILogger? logger = null) =>
            CreateContainerWriter(logger).AppendContainerToStream(luminaire, containerStream);

        private static IContainerBuilder CreateContainerWriter(ILogger? logger = null)
        {
            var fileHandler = new FileHandler();
            var xmlDtoSerializer = new XmlDtoSerializer();
            var xmlValidator = new XmlValidator();
            return new ContainerBuilder(fileHandler, xmlDtoSerializer, xmlValidator, logger);
        }
    }
}
