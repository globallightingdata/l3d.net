using System;
using System.IO;
using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using L3D.Net.Tests.Context;
using L3D.Net.XML;
using L3D.Net.XML.V0_9_1;
using L3D.Net.XML.V0_9_1.Dto;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests
{
    [TestFixture]
    public class ContainerBuilderTests
    {
        class Context : IContextWithFileHandler
        {
            public IFileHandler FileHandler { get; }
            public IXmlDtoConverter Converter { get; }
            public IXmlDtoSerializer Serializer { get; }
            public IXmlValidator Validator { get; }
            public ILogger Logger { get; }
            public ContainerBuilder Builder { get; }

            public Context()
            {
                FileHandler = Substitute.For<IFileHandler>();
                Converter = Substitute.For<IXmlDtoConverter>();
                Serializer = Substitute.For<IXmlDtoSerializer>();
                Validator = Substitute.For<IXmlValidator>();
                Validator.ValidateFile(Arg.Any<string>(), out Arg.Any<L3dXmlVersion>(), Arg.Any<ILogger>()).Returns(true);
                Logger = LoggerSubstitute.Create();
                Builder = new ContainerBuilder(FileHandler, Converter, Serializer, Validator, Logger);
            }
        }

        class ContextOptions : IContextOptionsWithFileHandler
        {
            public ContextOptions(Context context)
            {
                Context = context;
            }

            public Context Context { get; }

            IContextWithFileHandler IContextOptionsWithFileHandler.Context => Context;
        }

        Context CreateContext(Func<ContextOptions, ContextOptions> options = null)
        {
            var context = new Context();
            options?.Invoke(new ContextOptions(context));
            return context;
        }

        Luminaire CreateSimpleLuminaire()
        {
            var builder = Builder.NewLuminaire();
            builder.BuildExample000();
            return builder.Luminaire;
        }

        Luminaire CreateComplexLuminaire()
        {
            var builder = Builder.NewLuminaire();
            builder.BuildExample002();
            return builder.Luminaire;
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenFileHandlerIsNull()
        {
            Action action = () => new ContainerBuilder(
                null,
                Substitute.For<IXmlDtoConverter>(),
                Substitute.For<IXmlDtoSerializer>(),
                Substitute.For<IXmlValidator>(),
                Substitute.For<ILogger>()
            );

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenDtoConverterIsNull()
        {
            Action action = () => new ContainerBuilder(
                Substitute.For<IFileHandler>(),
                null,
                Substitute.For<IXmlDtoSerializer>(),
                Substitute.For<IXmlValidator>(),
                Substitute.For<ILogger>()
            );

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenDtoSerializerIsNull()
        {
            Action action = () => new ContainerBuilder(
                Substitute.For<IFileHandler>(),
                Substitute.For<IXmlDtoConverter>(),
                null,
                Substitute.For<IXmlValidator>(),
                Substitute.For<ILogger>()
            );

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenXmlValidatorIsNull()
        {
            Action action = () => new ContainerBuilder(
                Substitute.For<IFileHandler>(),
                Substitute.For<IXmlDtoConverter>(),
                Substitute.For<IXmlDtoSerializer>(),
                null,
                Substitute.For<ILogger>()
            );

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CreateContainer_ShouldThrowArgumentNullException_WhenLuminaireIsNull()
        {
            var context = CreateContext();

            Action action = () => context.Builder.CreateContainer(null, Guid.NewGuid().ToString());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void CreateContainer_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string path)
        {
            var context = CreateContext();

            Action action = () => context.Builder.CreateContainer(CreateSimpleLuminaire(), path);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void CreateContainer_ShouldCallFileHandlerCreateTemporaryDirectoryScope()
        {
            var context = CreateContext();

            context.Builder.CreateContainer(CreateSimpleLuminaire(), Guid.NewGuid().ToString());

            context.FileHandler.Received(1).CreateContainerDirectory();
        }

        [Test]
        public void CreateContainer_ShouldCleanUpContainerDirectory()
        {
            IContainerDirectory scope = null;
            var context = CreateContext(options => options.WithTemporaryDirectoryScope(out scope, out _));

            context.FileHandler.CreateContainerDirectory().Returns(scope);

            context.Builder.CreateContainer(CreateSimpleLuminaire(), Guid.NewGuid().ToString());

            scope.Received(1).CleanUp();
        }

        [Test]
        public void CreateContainer_ShouldCallFileHandlerCopyModelFiles_ForEveryGeometryDefinition_WithCorrectPath()
        {
            string tempPath = null;
            var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
            var luminaire = CreateComplexLuminaire();

            context.Builder.CreateContainer(luminaire, Guid.NewGuid().ToString());

            foreach (var geometryDefinition in luminaire.GeometryDefinitions)
            {
                var expectedPath = Path.Combine(tempPath, geometryDefinition.Id);
                var expectedModel = geometryDefinition.Model;

                context.FileHandler.Received(1).CopyModelFiles(Arg.Is(expectedModel), Arg.Is(expectedPath));
            }

            luminaire.GeometryDefinitions.Count.Should().BePositive();
            context.FileHandler.Received(luminaire.GeometryDefinitions.Count)
                .CopyModelFiles(Arg.Any<IModel3D>(), Arg.Any<string>());
        }

        [Test]
        public void CreateContainer_ShouldCallXmlDtoConverterConvertForLuminaire()
        {
            var context = CreateContext();
            var luminaire = CreateComplexLuminaire();

            context.Builder.CreateContainer(luminaire, Guid.NewGuid().ToString());

            context.Converter.Received(1).Convert(Arg.Is(luminaire));
        }

        [Test]
        public void CreateContainer_ShouldCallXmlDtoSerializerSerialize_ForConvertedDto_WithCorrectCorrectPath()
        {
            string tempPath = null;
            var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
            var luminaire = CreateComplexLuminaire();
            var expectedDto = new LuminaireDto();
            var expectedPath = Path.Combine(tempPath, Constants.L3dXmlFilename);

            context.Converter.Convert(Arg.Is(luminaire)).Returns(expectedDto);

            context.Builder.CreateContainer(luminaire, Guid.NewGuid().ToString());

            context.Serializer.Received(1).Serialize(Arg.Is(expectedDto), Arg.Is(expectedPath));
        }

        [Test]
        public void CreateContainer_ShouldCallXmlValidatorValidate_ForCorrectPath()
        {
            string tempPath = null;
            var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
            var luminaire = CreateComplexLuminaire();
            var expectedPath = Path.Combine(tempPath, Constants.L3dXmlFilename);

            context.Builder.CreateContainer(luminaire, Guid.NewGuid().ToString());

            context.Validator.Received(1).ValidateFile(Arg.Is(expectedPath), out Arg.Any<L3dXmlVersion>(), Arg.Any<ILogger>());
        }

        [Test]
        public void CreateContainer_ShouldCallNotCatch_WhenXmlValidatorValidateThrows()
        {
            var message = Guid.NewGuid().ToString();
            var context = CreateContext();
            context.Validator.When(v => v.ValidateFile(Arg.Any<string>(), out Arg.Any<L3dXmlVersion>(), Arg.Any<ILogger>()))
                .Do(_ => throw new Exception(message));

            Action action = () => context.Builder.CreateContainer(CreateComplexLuminaire(), Guid.NewGuid().ToString());

            action.Should().Throw<Exception>().WithMessage(message);
        }

        [Test]
        public void CreateContainer_ShouldCallFileHandlerCreateContianerFromDirectory_WithCorrectPath()
        {
            string tempPath = null;
            var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
            var luminaire = CreateComplexLuminaire();
            var containerPath = Guid.NewGuid().ToString();

            context.Builder.CreateContainer(luminaire, containerPath);

            context.FileHandler.Received(1).CreateContainerFromDirectory(Arg.Is(tempPath), Arg.Is(containerPath));
        }
    }
}