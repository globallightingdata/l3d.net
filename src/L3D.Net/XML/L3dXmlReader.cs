using System;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_9_0;
using Microsoft.Extensions.Logging;

namespace L3D.Net.XML
{
    internal class L3dXmlReader : IL3dXmlReader
    {
        private readonly IXmlValidator _validator;
        private readonly ILogger _logger;

        public L3dXmlReader(IXmlValidator validator, ILogger logger)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger;
        }

        public Luminaire Read(string filename, string workingDirectory)
        {
            if (!_validator.ValidateFile(filename, out var version, _logger))
                throw new Exception($"Failed to validate file '{filename}'!");

            switch (version)
            {
                case L3dXmlVersion.V0_9_0: return ReadV0_9_0(filename, workingDirectory);
            }
            
            throw new Exception($"Unknown version of the l3d xml: '{version}'");
        }

        private Luminaire ReadV0_9_0(string filepath, string workingDirectory)
        {
            var xmlSerializer = new XmlDtoSerializer();
            var luminaireConstructor = new LuminaireFromDtoConstructor();
            var luminaireDto = xmlSerializer.Deserialize(filepath);
            var builder = Builder.NewLuminaire(_logger);
            var luminaire = luminaireConstructor.BuildLuminaireFromDto(builder, luminaireDto, workingDirectory);
            return luminaire;
        }
    }
}