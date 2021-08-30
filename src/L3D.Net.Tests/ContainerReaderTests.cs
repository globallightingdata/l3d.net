using System;
using System.IO;
using FluentAssertions;
using L3D.Net.API.Dto;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UnusedMethodReturnValue.Local

namespace L3D.Net.Tests
{
    [TestFixture]
    public class ContainerReaderTests
    {
        class Context
        {
            public IFileHandler FileHandler { get; }
            public IL3dXmlReader L3dXmlReader { get; }
            public IApiDtoConverter Converter { get; }
            public ContainerReader Reader { get; }

            public Context()
            {
                FileHandler = Substitute.For<IFileHandler>();
                L3dXmlReader = Substitute.For<IL3dXmlReader>();
                Converter = Substitute.For<IApiDtoConverter>();
                Reader = new ContainerReader(FileHandler, L3dXmlReader, Converter);
            }
        }

        class ContextOptions
        {
            private readonly Context _context;

            public ContextOptions(Context context)
            {
                _context = context;
            }

            public ContextOptions WithTemporaryScopeWorkingDirectory(out IContainerDirectory scope,
                out string workingDirectory)
            {
                workingDirectory = Guid.NewGuid().ToString();
                scope = Substitute.For<IContainerDirectory>();
                scope.Path.Returns(workingDirectory);
                _context.FileHandler.CreateContainerDirectory().Returns(scope);
                return this;
            }

            public ContextOptions WithConvertedDto(out LuminaireDto luminaire)
            {
                luminaire = new LuminaireDto();

                _context.Converter.Convert(Arg.Any<Luminaire>(), Arg.Any<string>())
                    .Returns(luminaire);

                return this;
            }
        }

        private Context CreateContext(Action<ContextOptions> options = null)
        {
            var context = new Context();

            context.L3dXmlReader.Read(Arg.Any<string>(), Arg.Any<string>()).Returns(new Luminaire());
            context.Converter
                .Convert(Arg.Any<Luminaire>(), Arg.Any<string>())
                .Returns(_ => new LuminaireDto());

            options?.Invoke(new ContextOptions(context));

            return context;
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenFileHandlerIsNull()
        {
            Action action = () => new ContainerReader(
                null,
                Substitute.For<IL3dXmlReader>(),
                Substitute.For<IApiDtoConverter>()
            );

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenL3dXmlReaderIsNull()
        {
            Action action = () => new ContainerReader(
                Substitute.For<IFileHandler>(),
                null,
                Substitute.For<IApiDtoConverter>()
            );

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenApiDtoConverterIsNull()
        {
            Action action = () => new ContainerReader(
                Substitute.For<IFileHandler>(),
                Substitute.For<IL3dXmlReader>(),
                null
            );

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void Read_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string containerPath)
        {
            var context = CreateContext();

            Action action = () => context.Reader.Read(containerPath);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Read_ShouldCallFileHandlerCreateTemporaryDirectoryScope()
        {
            var context = CreateContext();

            context.Reader.Read(Guid.NewGuid().ToString());

            context.FileHandler.Received(1).CreateContainerDirectory();
        }

        [Test]
        public void Read_ShouldCallFileHandlerExtractContainerToDirectory_WithCorrectPath()
        {
            string workingDirectory = null;
            var context = CreateContext(options =>
                options.WithTemporaryScopeWorkingDirectory(out _, out workingDirectory));
            var containerPath = Guid.NewGuid().ToString();
            context.Reader.Read(containerPath);

            context.FileHandler.Received(1)
                .ExtractContainerToDirectory(Arg.Is(containerPath), Arg.Is(workingDirectory));
        }

        [Test]
        public void Read_ShouldCallL3dXmlReaderRead_WithCorrectXmlFilePath()
        {
            string workingDirectory = null;
            var context = CreateContext(options =>
                options.WithTemporaryScopeWorkingDirectory(out _, out workingDirectory));
            var xmlPath = Path.Combine(workingDirectory, Constants.L3dXmlFilename);
            context.Reader.Read(Guid.NewGuid().ToString());

            context.L3dXmlReader.Received(1).Read(xmlPath,  Arg.Any<string>());
        }

        [Test]
        public void Read_ShouldCallL3dXmlReaderRead_WithCorrectWorkingPath()
        {
            string workingDirectory = null;
            var context = CreateContext(options =>
                options.WithTemporaryScopeWorkingDirectory(out _, out workingDirectory));
            context.Reader.Read(Guid.NewGuid().ToString());

            context.L3dXmlReader.Received(1).Read(Arg.Any<string>(), workingDirectory);
        }

        [Test]
        public void Read_ShouldCallApiDtoConverterConvert()
        {
            Luminaire luminaire = new Luminaire();
            var context = CreateContext();
            context.L3dXmlReader.Read(Arg.Any<string>(), Arg.Any<string>()).Returns(luminaire);

            context.Reader.Read(Guid.NewGuid().ToString());

            context.Converter.Received(1).Convert(Arg.Is(luminaire), Arg.Any<string>());
        }

        [Test]
        public void Read_ShouldReturnConvertedDto()
        {
            LuminaireDto luminaire = null;
            var context = CreateContext(options => options.WithConvertedDto(out luminaire));

            var readLuminaire = context.Reader.Read(Guid.NewGuid().ToString());
            readLuminaire.Should().Be(luminaire);
        }

        [Test]
        public void Read_ShouldCallContainerDirectoryCleanUp_WhenNoErrorOccured()
        {
            IContainerDirectory scope = null;
            var context = CreateContext(options => options.WithTemporaryScopeWorkingDirectory(out scope, out _));

            context.Reader.Read(Guid.NewGuid().ToString());

            scope.Received(1).CleanUp();
        }

        [Test]
        public void
            Read_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenFileHandlerExtractContainerToDirectoryThrows()
        {
            IContainerDirectory scope = null;
            var context = CreateContext(options => options
                .WithTemporaryScopeWorkingDirectory(out scope, out _));

            var message = Guid.NewGuid().ToString();
            
            context.FileHandler
                .When(handler => handler.ExtractContainerToDirectory(Arg.Any<string>(), Arg.Any<string>()))
                .Throw(new Exception(message));

            Action action = () => context.Reader.Read(Guid.NewGuid().ToString());

            action.Should().Throw<Exception>().WithMessage(message);
            scope.Received(1).CleanUp();
        }

        [Test]
        public void Read_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenXmlValidatorValidateFileThrows()
        {
            IContainerDirectory scope = null;
            var context = CreateContext(options => options
                .WithTemporaryScopeWorkingDirectory(out scope, out _));

            var message = Guid.NewGuid().ToString();
            context.L3dXmlReader.Read(Arg.Any<string>(), Arg.Any<string>()).Throws(new Exception(message));

            Action action = () => context.Reader.Read(Guid.NewGuid().ToString());

            action.Should().Throw<Exception>().WithMessage(message);
            scope.Received(1).CleanUp();
        }

        [Test]
        public void Read_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenApiDtoConverterConvertThrows()
        {
            IContainerDirectory scope = null;
            var context = CreateContext(options => options
                .WithTemporaryScopeWorkingDirectory(out scope, out _));

            var message = Guid.NewGuid().ToString();
            
            context.Converter
                .When(converter => converter.Convert(Arg.Any<Luminaire>(), Arg.Any<string>()))
                .Throw(new Exception(message));

            Action action = () => context.Reader.Read(Guid.NewGuid().ToString());

            action.Should().Throw<Exception>().WithMessage(message);
            scope.Received(1).CleanUp();
        }
    }
}