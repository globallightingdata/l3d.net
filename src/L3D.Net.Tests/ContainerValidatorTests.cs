using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UnusedMethodReturnValue.Local

namespace L3D.Net.Tests;

[TestFixture]
class ContainerValidatorTests
{
    class Context
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
    }

    private static Context CreateContext(Action<ContextOptions> options = null)
    {
        var context = new Context();

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
        var action = () => _ = new ContainerValidator(null,
            Substitute.For<IXmlValidator>(),
            Substitute.For<ILogger>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenXmlValidatorIsNull()
    {
        var action = () => _ = new ContainerValidator(Substitute.For<IFileHandler>(),
            null,
            Substitute.For<ILogger>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldNotThrowArgumentNullException_WhenLoggerIsNull()
    {
        var action = () => _ = new ContainerValidator(Substitute.For<IFileHandler>(),
            Substitute.For<IXmlValidator>(),
            null
        );

        action.Should().NotThrow();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string containerPath)
    {
        var context = CreateContext();

        Action action = () => context.ContainerValidator.Validate(containerPath);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyByteArrayValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(byte[] containerBytes)
    {
        var context = CreateContext();

        Action action = () => context.ContainerValidator.Validate(containerBytes);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldCallFileHandlerCreateTemporaryDirectoryScope(ContainerTypeToTest containerTypeToTest)
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
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.FileHandler.Received(1).CreateContainerDirectory();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldCallFileHandlerExtractContainerToDirectory_WithCorrectPath(ContainerTypeToTest containerTypeToTest)
    {
        string workingDirectory = null;
        var context = CreateContext(options =>
            options.WithTemporaryScopeWorkingDirectory(out _, out workingDirectory));
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                var containerPath = Guid.NewGuid().ToString();
                context.ContainerValidator.Validate(containerPath);

                context.FileHandler.Received(1)
                    .ExtractContainerToDirectory(Arg.Is(containerPath), Arg.Is(workingDirectory));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] { 0, 1, 2, 3, 4 };
                context.ContainerValidator.Validate(containerBytes);

                context.FileHandler.Received(1)
                    .ExtractContainerToDirectory(Arg.Is(containerBytes), Arg.Is(workingDirectory));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldCallXmlValidatorValidateFile_WithCorrectXmlFilePath(ContainerTypeToTest containerTypeToTest)
    {
        string workingDirectory = null;
        var context = CreateContext(options =>
            options.WithTemporaryScopeWorkingDirectory(out _, out workingDirectory));
        var xmlPath = Path.Combine(workingDirectory, Constants.L3dXmlFilename);
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.XmlValidator.Received(1).ValidateFile(xmlPath, context.Logger);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldCallContainerDirectoryCleanUp_WhenNoErrorOccured(ContainerTypeToTest containerTypeToTest)
    {
        IContainerDirectory scope = null;
        var context = CreateContext(options => options.WithTemporaryScopeWorkingDirectory(out scope, out _));

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        scope.Received(1).CleanUp();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void
        Validate_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenFileHandlerExtractContainerToDirectoryThrows(ContainerTypeToTest containerTypeToTest)
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
                action = () => context.ContainerValidator.Validate(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                context.FileHandler
                    .When(handler => handler.ExtractContainerToDirectory(Arg.Any<byte[]>(), Arg.Any<string>()))
                    .Throw(new Exception(message));
                action = () => context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        action.Should().Throw<Exception>().WithMessage(message);
        scope.Received(1).CleanUp();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenXmlValidatorValidateFileThrows(ContainerTypeToTest containerTypeToTest)
    {
        IContainerDirectory scope = null;
        var context = CreateContext(options => options
            .WithTemporaryScopeWorkingDirectory(out scope, out _));

        var message = Guid.NewGuid().ToString();
        context.XmlValidator.ValidateFile(Arg.Any<string>(), context.Logger).Throws(new Exception(message));

        Action action = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => () => context.ContainerValidator.Validate(Guid.NewGuid().ToString()),
            ContainerTypeToTest.Bytes => () => context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        action.Should().Throw<Exception>().WithMessage(message);
        scope.Received(1).CleanUp();
    }
}