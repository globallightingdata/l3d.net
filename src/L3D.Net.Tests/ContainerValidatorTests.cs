using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UnusedMethodReturnValue.Local

namespace L3D.Net.Tests;

[TestFixture]
public class ContainerValidatorTests
{
    private class Context
    {
        public IFileHandler FileHandler { get; }
        public IXmlValidator XmlValidator { get; }
        public ILogger Logger { get; }
        public ContainerValidator ContainerValidator { get; }

        public Context()
        {
            FileHandler = Substitute.For<IFileHandler>();
            XmlValidator = Substitute.For<IXmlValidator>();
            Logger = LoggerSubstitute.Create();
            ContainerValidator = new ContainerValidator(FileHandler, XmlValidator, Logger);
            FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache { StructureXml = Stream.Null });
            FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache { StructureXml = Stream.Null });
            FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache { StructureXml = Stream.Null });
        }
    }

    private static Context CreateContext()
    {
        var context = new Context();

        return context;
    }

    public enum ContainerTypeToTest
    {
        Path,
        Bytes,
        Stream
    }

    public static IEnumerable<ContainerTypeToTest> ContainerTypeToTestEnumValues => Enum.GetValues<ContainerTypeToTest>();

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileHandlerIsNull()
    {
        var action = () => _ = new ContainerValidator(null!,
            Substitute.For<IXmlValidator>(),
            Substitute.For<ILogger>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenXmlValidatorIsNull()
    {
        var action = () => _ = new ContainerValidator(Substitute.For<IFileHandler>(),
            null!,
            Substitute.For<ILogger>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldNotThrowArgumentNullException_WhenLoggerIsNull()
    {
        var action = () => _ = new ContainerValidator(Substitute.For<IFileHandler>(),
            Substitute.For<IXmlValidator>(),
            null!
        );

        action.Should().NotThrow();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string containerPath)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.Validate(containerPath);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyByteArrayValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(byte[] containerBytes)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.Validate(containerBytes);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStreamValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(Stream containerStream)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.Validate(containerStream);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldCallFileHandlerExtractContainer(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                var containerPath = Guid.NewGuid().ToString();
                context.ContainerValidator.Validate(containerPath);

                context.FileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerPath));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] { 0, 1, 2, 3, 4 };
                context.ContainerValidator.Validate(containerBytes);

                context.FileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerBytes));
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms);

                    context.FileHandler.Received(1)
                        .ExtractContainer(Arg.Is(ms));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldCallXmlValidatorValidateFile_WithCorrectXmlFilePath(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 });
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.XmlValidator.Received(1).ValidateStream(Arg.Any<Stream>(), context.Logger);
    }
}