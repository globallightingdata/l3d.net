using FluentAssertions;
using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_11_0;
using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests;

[TestFixture]
public class ContainerBuilderTests
{
    private class Context
    {
        public IFileHandler FileHandler { get; }
        private IXmlDtoSerializer Serializer { get; }
        public IXmlValidator Validator { get; }
        public ContainerBuilder Builder { get; }

        public Context()
        {
            FileHandler = Substitute.For<IFileHandler>();
            Serializer = Substitute.For<IXmlDtoSerializer>();
            Validator = Substitute.For<IXmlValidator>();
            Validator.ValidateStream(Arg.Any<Stream>()).Returns(Array.Empty<ValidationHint>());
            Builder = new ContainerBuilder(FileHandler, Serializer, Validator);
        }
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
            Substitute.For<IXmlValidator>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenDtoSerializerIsNull()
    {
        var action = () => _ = new ContainerBuilder(
            Substitute.For<IFileHandler>(),
            null!,
            Substitute.For<IXmlValidator>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenXmlValidatorIsNull()
    {
        var action = () => _ = new ContainerBuilder(
            Substitute.For<IFileHandler>(),
            Substitute.For<IXmlDtoSerializer>(),
            null!
        );

        action.Should().Throw<ArgumentNullException>();
    }

    #region CreateContainerFile

    [Test]
    public void CreateContainerFile_ShouldThrowArgumentNullException_WhenLuminaireIsNull()
    {
        var context = new Context();

        var action = () => context.Builder.CreateContainerFile(null!, Guid.NewGuid().ToString());

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void CreateContainerFile_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string path)
    {
        var context = new Context();

        var action = () => context.Builder.CreateContainerFile(CreateSimpleLuminaire(), path);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateContainerFile_ShouldCallFileHandlerCopyModelFiles_ForEveryGeometryDefinition()
    {
        var context = new Context();
        var luminaire = CreateComplexLuminaire();

        context.Builder.CreateContainerFile(luminaire, Guid.NewGuid().ToString());

        foreach (var geometryDefinition in luminaire.GeometryDefinitions)
        {
            var expectedModel = geometryDefinition.Model;

            context.FileHandler.Received(1).AddModelFilesToCache(Arg.Is(expectedModel!), Arg.Is(geometryDefinition.GeometryId), Arg.Any<ContainerCache>());
        }

        luminaire.GeometryDefinitions.Count.Should().BePositive();
        context.FileHandler.Received(luminaire.GeometryDefinitions.Count)
            .AddModelFilesToCache(Arg.Any<IModel3D>(), Arg.Any<string>(), Arg.Any<ContainerCache>());
    }

    [Test]
    public void CreateContainerFile_ShouldCallXmlValidatorValidate_ForCorrectPath()
    {
        var context = new Context();
        var luminaire = CreateComplexLuminaire();

        context.Builder.CreateContainerFile(luminaire, Guid.NewGuid().ToString());

        context.Validator.Received(1).ValidateStream(Arg.Any<Stream>());
    }

    [Test]
    public void CreateContainerFile_ShouldCallNotCatch_WhenXmlValidatorValidateThrows()
    {
        var message = Guid.NewGuid().ToString();
        var context = new Context();
        context.Validator.When(v => v.ValidateStream(Arg.Any<Stream>()))
            .Do(_ => throw new Exception(message));

        var action = () => context.Builder.CreateContainerFile(CreateComplexLuminaire(), Guid.NewGuid().ToString());

        action.Should().Throw<Exception>().WithMessage(message);
    }

    [Test]
    public void CreateContainerFile_ShouldCallFileHandlerCreateContianerFromDirectory_WithCorrectPath()
    {
        var context = new Context();
        var luminaire = CreateComplexLuminaire();
        var containerPath = Guid.NewGuid().ToString();

        context.Builder.CreateContainerFile(luminaire, containerPath);

        context.FileHandler.Received(1).CreateContainerFile(Arg.Any<ContainerCache>(), Arg.Is(containerPath));
    }

    #endregion

    #region CreateContainerByteArray

    [Test]
    public void CreateContainerByteArray_ShouldThrowArgumentNullException_WhenLuminaireIsNull()
    {
        var context = new Context();

        var action = () => context.Builder.CreateContainerByteArray(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallFileHandlerCopyModelFiles_ForEveryGeometryDefinition()
    {
        var context = new Context();
        var luminaire = CreateComplexLuminaire();

        context.Builder.CreateContainerByteArray(luminaire);

        foreach (var geometryDefinition in luminaire.GeometryDefinitions)
        {
            var expectedModel = geometryDefinition.Model;

            context.FileHandler.Received(1).AddModelFilesToCache(Arg.Is(expectedModel!), Arg.Is(geometryDefinition.GeometryId), Arg.Any<ContainerCache>());
        }

        luminaire.GeometryDefinitions.Count.Should().BePositive();
        context.FileHandler.Received(luminaire.GeometryDefinitions.Count)
            .AddModelFilesToCache(Arg.Any<IModel3D>(), Arg.Any<string>(), Arg.Any<ContainerCache>());
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallXmlValidatorValidate()
    {
        var context = new Context();
        var luminaire = CreateComplexLuminaire();

        context.Builder.CreateContainerByteArray(luminaire);

        context.Validator.Received(1).ValidateStream(Arg.Any<Stream>());
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallNotCatch_WhenXmlValidatorValidateThrows()
    {
        var message = Guid.NewGuid().ToString();
        var context = new Context();
        context.Validator.When(v => v.ValidateStream(Arg.Any<Stream>()))
            .Do(_ => throw new Exception(message));

        var action = () => context.Builder.CreateContainerByteArray(CreateComplexLuminaire());

        action.Should().Throw<Exception>().WithMessage(message);
    }

    [Test]
    public void CreateContainerByteArray_ShouldCallFileHandlerCreateContianerFromDirectory()
    {
        var context = new Context();
        var luminaire = CreateComplexLuminaire();

        context.Builder.CreateContainerByteArray(luminaire);

        context.FileHandler.Received(1).CreateContainerByteArray(Arg.Any<ContainerCache>());
    }

    #endregion
}