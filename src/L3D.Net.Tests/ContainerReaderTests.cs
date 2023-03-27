using FluentAssertions;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Tests;

[TestFixture]
public class ContainerReaderTests
{
    private ContainerReader _reader;
    private IFileHandler _fileHandler;
    private IL3DXmlReader _l3DXmlReader;

    [SetUp]
    public void SetUp()
    {
        _fileHandler = Substitute.For<IFileHandler>();
        _l3DXmlReader = Substitute.For<IL3DXmlReader>();

        _reader = new ContainerReader(_fileHandler, _l3DXmlReader);
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
        var action = () => _ = new ContainerReader(
            null,
            Substitute.For<IL3DXmlReader>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenL3dXmlReaderIsNull()
    {
        var action = () => _ = new ContainerReader(
            Substitute.For<IFileHandler>(),
            null
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void Read_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string containerPath)
    {
        var action = () => _reader.Read(containerPath);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyByteArrayValues))]
    public void Read_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(byte[] containerBytes)
    {
        var action = () => _reader.Read(containerBytes);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallFileHandlerCreateTemporaryDirectoryScope(ContainerTypeToTest containerTypeToTest)
    {
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                _reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        _fileHandler.Received(1).CreateContainerDirectory();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallFileHandlerExtractContainerToDirectory_WithCorrectPath(ContainerTypeToTest containerTypeToTest)
    {
        var workingDirectory = Guid.NewGuid().ToString();

        var containerDir = Substitute.For<IContainerDirectory>();
        containerDir.Path.Returns(workingDirectory);
        _fileHandler.CreateContainerDirectory().Returns(containerDir);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                var containerPath = Guid.NewGuid().ToString();
                _reader.Read(containerPath);

                _fileHandler.Received(1)
                    .ExtractContainerToDirectory(Arg.Is(containerPath), Arg.Is(workingDirectory));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] { 0, 1, 2, 3, 4 };
                _reader.Read(containerBytes);

                _fileHandler.Received(1)
                    .ExtractContainerToDirectory(Arg.Is(containerBytes), Arg.Is(workingDirectory));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallL3dXmlReaderRead_WithCorrectXmlFilePath(ContainerTypeToTest containerTypeToTest)
    {
        var workingDirectory = Guid.NewGuid().ToString();

        var containerDir = Substitute.For<IContainerDirectory>();
        containerDir.Path.Returns(workingDirectory);
        _fileHandler.CreateContainerDirectory().Returns(containerDir);

        var xmlPath = Path.Combine(workingDirectory, Constants.L3dXmlFilename);
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                _reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        _l3DXmlReader.Received(1).Read(xmlPath, Arg.Any<string>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallL3dXmlReaderRead_WithCorrectWorkingPath(ContainerTypeToTest containerTypeToTest)
    {
        var workingDirectory = Guid.NewGuid().ToString();

        var containerDir = Substitute.For<IContainerDirectory>();
        containerDir.Path.Returns(workingDirectory);
        _fileHandler.CreateContainerDirectory().Returns(containerDir);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                _reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        _l3DXmlReader.Received(1).Read(Arg.Any<string>(), workingDirectory);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallContainerDirectoryCleanUp_WhenNoErrorOccured(ContainerTypeToTest containerTypeToTest)
    {
        var workingDirectory = Guid.NewGuid().ToString();

        var containerDir = Substitute.For<IContainerDirectory>();
        containerDir.Path.Returns(workingDirectory);
        _fileHandler.CreateContainerDirectory().Returns(containerDir);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                _reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        containerDir.Received(1).CleanUp();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void
        Read_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenFileHandlerExtractContainerToDirectoryThrows(ContainerTypeToTest containerTypeToTest)
    {
        var workingDirectory = Guid.NewGuid().ToString();

        var containerDir = Substitute.For<IContainerDirectory>();
        containerDir.Path.Returns(workingDirectory);
        _fileHandler.CreateContainerDirectory().Returns(containerDir);

        var message = Guid.NewGuid().ToString();

        Action action;
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _fileHandler
                    .When(handler => handler.ExtractContainerToDirectory(Arg.Any<string>(), Arg.Any<string>()))
                    .Throw(new Exception(message));
                action = () => _reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                _fileHandler
                    .When(handler => handler.ExtractContainerToDirectory(Arg.Any<byte[]>(), Arg.Any<string>()))
                    .Throw(new Exception(message));
                action = () => _reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        action.Should().Throw<Exception>().WithMessage(message);
        containerDir.Received(1).CleanUp();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallContainerDirectoryCleanUp_AndNotCatch_WhenXmlValidatorValidateFileThrows(ContainerTypeToTest containerTypeToTest)
    {
        var workingDirectory = Guid.NewGuid().ToString();

        var containerDir = Substitute.For<IContainerDirectory>();
        containerDir.Path.Returns(workingDirectory);
        _fileHandler.CreateContainerDirectory().Returns(containerDir);

        var message = Guid.NewGuid().ToString();
        _l3DXmlReader.Read(Arg.Any<string>(), Arg.Any<string>()).Throws(new Exception(message));

        Action action = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => () => _reader.Read(Guid.NewGuid().ToString()),
            ContainerTypeToTest.Bytes => () => _reader.Read(new byte[] { 0, 1, 2, 3, 4 }),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        action.Should().Throw<Exception>().WithMessage(message);
        containerDir.Received(1).CleanUp();
    }
}