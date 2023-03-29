using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using L3D.Net.Tests.Context;
using L3D.Net.XML.V0_11_0;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests;

[TestFixture]
public class ContainerBuilderTests
{
    private class Context : IContextWithFileHandler
    {
        public IFileHandler FileHandler { get; }
        private IXmlDtoSerializer Serializer { get; }
        public IXmlValidator Validator { get; }
        private ILogger Logger { get; }
        public ContainerBuilder Builder { get; }

        public Context()
        {
            FileHandler = Substitute.For<IFileHandler>();
            Serializer = Substitute.For<IXmlDtoSerializer>();
            Validator = Substitute.For<IXmlValidator>();
            Validator.ValidateFile(Arg.Any<string>(), Arg.Any<ILogger>()).Returns(true);
            Logger = LoggerSubstitute.Create();
            Builder = new ContainerBuilder(FileHandler, Serializer, Validator, Logger);
        }
    }

    private class ContextOptions : IContextOptionsWithFileHandler
    {
        public ContextOptions(Context context)
        {
            Context = context;
        }

        private Context Context { get; }

        IContextWithFileHandler IContextOptionsWithFileHandler.Context => Context;
    }

    private static Context CreateContext(Func<ContextOptions, ContextOptions>? options = null)
    {
        var context = new Context();
        options?.Invoke(new ContextOptions(context));
        return context;
    }

    static Luminaire CreateSimpleLuminaire()
    {
        var luminaire = new Luminaire();
        luminaire.BuildExample000();
        return luminaire;
    }

    static Luminaire CreateComplexLuminaire()
    {
        var luminaire = new Luminaire();
        luminaire.BuildExample002();
        return luminaire;
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileHandlerIsNull()
    {
        var action = () => _ = new ContainerBuilder(
            null!,
            Substitute.For<IXmlDtoSerializer>(),
            Substitute.For<IXmlValidator>(),
            Substitute.For<ILogger>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenDtoSerializerIsNull()
    {
        var action = () => _ = new ContainerBuilder(
            Substitute.For<IFileHandler>(),
            null!,
            Substitute.For<IXmlValidator>(),
            Substitute.For<ILogger>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenXmlValidatorIsNull()
    {
        var action = () => _ = new ContainerBuilder(
            Substitute.For<IFileHandler>(),
            Substitute.For<IXmlDtoSerializer>(),
            null!,
            Substitute.For<ILogger>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    #region CreateContainerFile

    [Test]
    public void CreateContainerFile_ShouldThrowArgumentNullException_WhenLuminaireIsNull()
    {
        var context = CreateContext();

        var action = () => context.Builder.CreateContainerFile(null!, Guid.NewGuid().ToString());

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void CreateContainerFile_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string path)
    {
        var context = CreateContext();

        var action = () => context.Builder.CreateContainerFile(CreateSimpleLuminaire(), path);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateContainerFile_ShouldCallFileHandlerCreateTemporaryDirectoryScope()
    {
        var context = CreateContext();

        context.Builder.CreateContainerFile(CreateSimpleLuminaire(), Guid.NewGuid().ToString());

        context.FileHandler.Received(1).CreateContainerDirectory();
    }

    [Test]
    public void CreateContainerFile_ShouldCleanUpContainerDirectory()
    {
        IContainerDirectory scope = null!;
        var context = CreateContext(options => options.WithTemporaryDirectoryScope(out scope, out _));

        context.FileHandler.CreateContainerDirectory().Returns(scope);

        context.Builder.CreateContainerFile(CreateSimpleLuminaire(), Guid.NewGuid().ToString());

        scope.Received(1).CleanUp();
    }

    [Test]
    public void CreateContainerFile_ShouldCallFileHandlerCopyModelFiles_ForEveryGeometryDefinition_WithCorrectPath()
    {
        string tempPath = null!;
        var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
        var luminaire = CreateComplexLuminaire();

        context.Builder.CreateContainerFile(luminaire, Guid.NewGuid().ToString());

        foreach (var geometryDefinition in luminaire.GeometryDefinitions)
        {
            var expectedPath = Path.Combine(tempPath, geometryDefinition.GeometryId);
            var expectedModel = geometryDefinition.Model;

            context.FileHandler.Received(1).CopyModelFiles(Arg.Is(expectedModel), Arg.Is(expectedPath));
        }

        luminaire.GeometryDefinitions.Count.Should().BePositive();
        context.FileHandler.Received(luminaire.GeometryDefinitions.Count)
            .CopyModelFiles(Arg.Any<IModel3D>(), Arg.Any<string>());
    }

    [Test]
    public void CreateContainerFile_ShouldCallXmlValidatorValidate_ForCorrectPath()
    {
        string tempPath = null!;
        var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
        var luminaire = CreateComplexLuminaire();
        var expectedPath = Path.Combine(tempPath, Constants.L3dXmlFilename);

        context.Builder.CreateContainerFile(luminaire, Guid.NewGuid().ToString());

        context.Validator.Received(1).ValidateFile(Arg.Is(expectedPath), Arg.Any<ILogger>());
    }

    [Test]
    public void CreateContainerFile_ShouldCallNotCatch_WhenXmlValidatorValidateThrows()
    {
        var message = Guid.NewGuid().ToString();
        var context = CreateContext();
        context.Validator.When(v => v.ValidateFile(Arg.Any<string>(), Arg.Any<ILogger>()))
            .Do(_ => throw new Exception(message));

        var action = () => context.Builder.CreateContainerFile(CreateComplexLuminaire(), Guid.NewGuid().ToString());

        action.Should().Throw<Exception>().WithMessage(message);
    }

    [Test]
    public void CreateContainerFile_ShouldCallFileHandlerCreateContianerFromDirectory_WithCorrectPath()
    {
        string tempPath = null!;
        var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
        var luminaire = CreateComplexLuminaire();
        var containerPath = Guid.NewGuid().ToString();

        context.Builder.CreateContainerFile(luminaire, containerPath);

        context.FileHandler.Received(1).CreateContainerFile(Arg.Is(tempPath), Arg.Is(containerPath));
    }

    #endregion

    #region CreateContainerByteArray

    [Test]
    public void CreateContainerByteArray_ShouldThrowArgumentNullException_WhenLuminaireIsNull()
    {
        var context = CreateContext();

        var action = () => context.Builder.CreateContainerByteArray(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallFileHandlerCreateTemporaryDirectoryScope()
    {
        var context = CreateContext();

        context.Builder.CreateContainerByteArray(CreateSimpleLuminaire());

        context.FileHandler.Received(1).CreateContainerDirectory();
    }

    [Test]
    public void CreateContainerByteArray_ShouldCleanUpContainerDirectory()
    {
        IContainerDirectory scope = null!;
        var context = CreateContext(options => options.WithTemporaryDirectoryScope(out scope, out _));

        context.FileHandler.CreateContainerDirectory().Returns(scope);

        context.Builder.CreateContainerByteArray(CreateSimpleLuminaire());

        scope.Received(1).CleanUp();
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallFileHandlerCopyModelFiles_ForEveryGeometryDefinition()
    {
        string tempPath = null!;
        var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
        var luminaire = CreateComplexLuminaire();

        context.Builder.CreateContainerByteArray(luminaire);

        foreach (var geometryDefinition in luminaire.GeometryDefinitions)
        {
            var expectedPath = Path.Combine(tempPath, geometryDefinition.GeometryId);
            var expectedModel = geometryDefinition.Model;

            context.FileHandler.Received(1).CopyModelFiles(Arg.Is(expectedModel), Arg.Is(expectedPath));
        }

        luminaire.GeometryDefinitions.Count.Should().BePositive();
        context.FileHandler.Received(luminaire.GeometryDefinitions.Count)
            .CopyModelFiles(Arg.Any<IModel3D>(), Arg.Any<string>());
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallXmlValidatorValidate()
    {
        string tempPath = null!;
        var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
        var luminaire = CreateComplexLuminaire();
        var expectedPath = Path.Combine(tempPath, Constants.L3dXmlFilename);

        context.Builder.CreateContainerByteArray(luminaire);

        context.Validator.Received(1).ValidateFile(Arg.Is(expectedPath), Arg.Any<ILogger>());
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallNotCatch_WhenXmlValidatorValidateThrows()
    {
        var message = Guid.NewGuid().ToString();
        var context = CreateContext();
        context.Validator.When(v => v.ValidateFile(Arg.Any<string>(), Arg.Any<ILogger>()))
            .Do(_ => throw new Exception(message));

        var action = () => context.Builder.CreateContainerByteArray(CreateComplexLuminaire());

        action.Should().Throw<Exception>().WithMessage(message);
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallFileHandlerCreateContianerFromDirectory()
    {
        string tempPath = null!;
        var context = CreateContext(options => options.WithTemporaryDirectoryScope(out _, out tempPath));
        var luminaire = CreateComplexLuminaire();

        context.Builder.CreateContainerByteArray(luminaire);

        context.FileHandler.Received(1).CreateContainerByteArray(Arg.Is(tempPath));
    }

    #endregion
}