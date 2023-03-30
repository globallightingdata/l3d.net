using FluentAssertions;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Tests;

[TestFixture]
public class ContainerReaderTests
{
    private ContainerReader _reader = null!;
    private IFileHandler _fileHandler = null!;
    private IL3DXmlReader _l3DXmlReader = null!;

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
        Bytes,
        Stream
    }

    public static IEnumerable<ContainerTypeToTest> ContainerTypeToTestEnumValues => Enum.GetValues<ContainerTypeToTest>();

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileHandlerIsNull()
    {
        var action = () => _ = new ContainerReader(
            null!,
            Substitute.For<IL3DXmlReader>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenL3dXmlReaderIsNull()
    {
        var action = () => _ = new ContainerReader(
            Substitute.For<IFileHandler>(),
            null!
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
    public void Read_ShouldCallFileHandlerExtractContainer(ContainerTypeToTest containerTypeToTest)
    {
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                var containerPath = Guid.NewGuid().ToString();
                _reader.Read(containerPath);

                _fileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerPath));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] { 0, 1, 2, 3, 4 };
                _reader.Read(containerBytes);

                _fileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerBytes));
                break;
            case ContainerTypeToTest.Stream:
                using (var stream = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    _reader.Read(stream);

                    _fileHandler.Received(1)
                        .ExtractContainer(Arg.Is(stream));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldCallL3dXmlReaderRead(ContainerTypeToTest containerTypeToTest)
    {
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _reader.Read(Guid.NewGuid().ToString());
                break;
            case ContainerTypeToTest.Bytes:
                _reader.Read(new byte[] { 0, 1, 2, 3, 4 });
                break;
            case ContainerTypeToTest.Stream:
                using (var stream = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                    _reader.Read(stream);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        _l3DXmlReader.Received(1).Read(Arg.Any<ContainerCache>());
    }
}