using System;
using System.Collections.Generic;
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

namespace L3D.Net.Tests;

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
        
    public enum ContainerTypeToTest
    {
        Path,
        Bytes
    }

    public static IEnumerable<ContainerTypeToTest> ContainerTypeToTestEnumValues => Enum.GetValues<ContainerTypeToTest>();

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

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void Read_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string containerPath)
    {
        var context = CreateContext();

        Action action = () => context.Reader.Read(containerPath);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyByteArrayValues))]
    public void Read_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(byte[] containerBytes)
    {
        var context = CreateContext();

        Action action = () => context.Reader.Read(containerBytes);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallFileHandlerCreateTemporaryDirectoryScope(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.Reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.FileHandler.Received(1).CreateContainerDirectory();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallFileHandlerExtractContainerToDirectory_WithCorrectPath(ContainerTypeToTest containerTypeToTest)
    {
        string workingDirectory = null;
        var context = CreateContext(options =>
            options.WithTemporaryScopeWorkingDirectory(out _, out workingDirectory));
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                var containerPath = Guid.NewGuid().ToString();
                context.Reader.Read(containerPath);

                context.FileHandler.Received(1)
                    .ExtractContainerToDirectory(Arg.Is(containerPath), Arg.Is(workingDirectory));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] { 0, 1, 2, 3, 4 };
                context.Reader.Read(containerBytes);

                context.FileHandler.Received(1)
                    .ExtractContainerToDirectory(Arg.Is(containerBytes), Arg.Is(workingDirectory));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallL3dXmlReaderRead_WithCorrectXmlFilePath(ContainerTypeToTest containerTypeToTest)
    {
        string workingDirectory = null;
        var context = CreateContext(options =>
            options.WithTemporaryScopeWorkingDirectory(out _, out workingDirectory));
        var xmlPath = Path.Combine(workingDirectory, Constants.L3dXmlFilename);
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.Reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.L3dXmlReader.Received(1).Read(xmlPath, Arg.Any<string>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallL3dXmlReaderRead_WithCorrectWorkingPath(ContainerTypeToTest containerTypeToTest)
    {
        string workingDirectory = null;
        var context = CreateContext(options =>
            options.WithTemporaryScopeWorkingDirectory(out _, out workingDirectory));
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.Reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.L3dXmlReader.Received(1).Read(Arg.Any<string>(), workingDirectory);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallApiDtoConverterConvert(ContainerTypeToTest containerTypeToTest)
    {
        Luminaire luminaire = new Luminaire();
        var context = CreateContext();
        context.L3dXmlReader.Read(Arg.Any<string>(), Arg.Any<string>()).Returns(luminaire);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.Reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.Converter.Received(1).Convert(Arg.Is(luminaire), Arg.Any<string>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldReturnConvertedDto(ContainerTypeToTest containerTypeToTest)
    {
        LuminaireDto luminaire = null;
        var context = CreateContext(options => options.WithConvertedDto(out luminaire));

        var readLuminaire = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => context.Reader.Read(Guid.NewGuid().ToString()),
            ContainerTypeToTest.Bytes => context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 }),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };
        readLuminaire.Should().Be(luminaire);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallContainerDirectoryCleanUp_WhenNoErrorOccured(ContainerTypeToTest containerTypeToTest)
    {
        IContainerDirectory scope = null;
        var context = CreateContext(options => options.WithTemporaryScopeWorkingDirectory(out scope, out _));

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.Reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        scope.Received(1).CleanUp();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void
        Read_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenFileHandlerExtractContainerToDirectoryThrows(ContainerTypeToTest containerTypeToTest)
    {
        IContainerDirectory scope = null;
        var context = CreateContext(options => options
            .WithTemporaryScopeWorkingDirectory(out scope, out _));

        var message = Guid.NewGuid().ToString();

        Action action;
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.FileHandler
                    .When(handler => handler.ExtractContainerToDirectory(Arg.Any<string>(), Arg.Any<string>()))
                    .Throw(new Exception(message));
                action = () => context.Reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.FileHandler
                    .When(handler => handler.ExtractContainerToDirectory(Arg.Any<byte[]>(), Arg.Any<string>()))
                    .Throw(new Exception(message));
                action = () => context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        action.Should().Throw<Exception>().WithMessage(message);
        scope.Received(1).CleanUp();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenXmlValidatorValidateFileThrows(ContainerTypeToTest containerTypeToTest)
    {
        IContainerDirectory scope = null;
        var context = CreateContext(options => options
            .WithTemporaryScopeWorkingDirectory(out scope, out _));

        var message = Guid.NewGuid().ToString();
        context.L3dXmlReader.Read(Arg.Any<string>(), Arg.Any<string>()).Throws(new Exception(message));

        Action action = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => () => context.Reader.Read(Guid.NewGuid().ToString()),
            ContainerTypeToTest.Bytes => () => context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 }),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        action.Should().Throw<Exception>().WithMessage(message);
        scope.Received(1).CleanUp();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenApiDtoConverterConvertThrows(ContainerTypeToTest containerTypeToTest)
    {
        IContainerDirectory scope = null;
        var context = CreateContext(options => options
            .WithTemporaryScopeWorkingDirectory(out scope, out _));

        var message = Guid.NewGuid().ToString();
            
        context.Converter
            .When(converter => converter.Convert(Arg.Any<Luminaire>(), Arg.Any<string>()))
            .Throw(new Exception(message));

        Action action = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => () => context.Reader.Read(Guid.NewGuid().ToString()),
            ContainerTypeToTest.Bytes => () => context.Reader.Read(new byte[] { 0, 1, 2, 3, 4 }),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        action.Should().Throw<Exception>().WithMessage(message);
        scope.Received(1).CleanUp();
    }
}