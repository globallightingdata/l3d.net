using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML;
using L3D.Net.XML.V0_10_0;
using System.IO;

namespace L3D.Net
{
    public class Writer : IWriter
    {
        public byte[] WriteToByteArray(Luminaire luminaire) =>
            CreateContainerWriter().CreateContainerByteArray(luminaire);

        public void WriteToFile(Luminaire luminaire, string containerPath) =>
            CreateContainerWriter().CreateContainerFile(luminaire, containerPath);

        public void WriteToStream(Luminaire luminaire, Stream containerStream) =>
            CreateContainerWriter().AppendContainerToStream(luminaire, containerStream);

        private static IContainerBuilder CreateContainerWriter()
        {
            var fileHandler = new FileHandler();
            var xmlDtoSerializer = new XmlDtoSerializer();
            var xmlValidator = new XmlValidator();
            return new ContainerBuilder(fileHandler, xmlDtoSerializer, xmlValidator);
        }
    }
}
