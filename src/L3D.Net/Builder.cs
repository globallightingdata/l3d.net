using System.Reflection;
using L3D.Net.Geometry;
using L3D.Net.Internal;
using L3D.Net.XML;
using L3D.Net.XML.V0_9_2;
using Microsoft.Extensions.Logging;

namespace L3D.Net
{
    public static class Builder
    {
        public static LuminaireBuilder NewLuminaire(ILogger logger = null)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var toolName = assembly.FullName;

            return new LuminaireBuilder(
                toolName,
                new ObjParser(),
                new ContainerBuilder(
                    new FileHandler(),
                    new XmlDtoConverter(),
                    new XmlDtoSerializer(),
                    new XmlValidator(),
                    logger
                ),
                logger
            );
        }
    }
}