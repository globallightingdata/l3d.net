using System;
using System.IO;
using L3D.Net.Abstract;
using L3D.Net.API.Dto;
using L3D.Net.Internal.Abstract;

namespace L3D.Net.Internal
{
    internal class ContainerReader : IContainerReader
    {
        private readonly IFileHandler _fileHandler;
        private readonly IL3dXmlReader _l3dXmlReader;
        private readonly IApiDtoConverter _apiDtoConverter;

        public ContainerReader(IFileHandler fileHandler, IL3dXmlReader l3dXmlReader, IApiDtoConverter apiDtoConverter)
        {
            _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
            _l3dXmlReader = l3dXmlReader ?? throw new ArgumentNullException(nameof(l3dXmlReader));
            _apiDtoConverter = apiDtoConverter ?? throw new ArgumentNullException(nameof(apiDtoConverter));
        }

        public LuminaireDto Read(string containerPath)
        {
            if (string.IsNullOrWhiteSpace(containerPath))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(containerPath));

            using var directoryScope = new ContainerDirectoryScope(_fileHandler.CreateContainerDirectory());
            _fileHandler.ExtractContainerToDirectory(containerPath, directoryScope.Directory);
            var structurePath = Path.Combine(directoryScope.Directory, Constants.L3dXmlFilename);
            var luminaire = _l3dXmlReader.Read(structurePath, directoryScope.Directory);
            return _apiDtoConverter.Convert(luminaire, directoryScope.Directory);
        }
    }
}