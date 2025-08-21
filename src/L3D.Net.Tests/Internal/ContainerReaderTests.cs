using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Exceptions;
using L3D.Net.Geometry;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests.Internal;

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
        _l3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire());

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
                    .ExtractContainerOrThrow(Arg.Is(containerPath));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] {0, 1, 2, 3, 4};
                _reader.Read(containerBytes);

                _fileHandler.Received(1)
                    .ExtractContainerOrThrow(Arg.Is(containerBytes));
                break;
            case ContainerTypeToTest.Stream:
                using (var stream = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    _reader.Read(stream);

                    _fileHandler.Received(1)
                        .ExtractContainerOrThrow(Arg.Is(stream));
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
                _reader.Read([0, 1, 2, 3, 4]);
                break;
            case ContainerTypeToTest.Stream:
                using (var stream = new MemoryStream([0, 1, 2, 3, 4]))
                    _reader.Read(stream);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        _l3DXmlReader.Received(1).Read(Arg.Any<ContainerCache>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Read_ShouldThrow_IfNothingCouldBeRead(ContainerTypeToTest containerTypeToTest)
    {
        _l3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns((Luminaire) null!);

        Action act = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => () => _reader.Read(Guid.NewGuid().ToString()),
            ContainerTypeToTest.Bytes => () => _reader.Read([0, 1, 2, 3, 4]),
            ContainerTypeToTest.Stream => () =>
            {
                using var stream = new MemoryStream([0, 1, 2, 3, 4]);
                _reader.Read(stream);
            },
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        act.Should().Throw<InvalidL3DException>().WithMessage("No L3D could be read");
    }

    private static List<string> ExampleFiles()
    {
        Setup.Initialize();
        return Setup.ExampleObjFiles.ToList();
    }

    [Test]
    [TestCaseSource(nameof(ExampleFiles))]
    public void ReadWrite_ShouldReturnSameData(string modelPath)
    {
        var objParser = new ObjParser();
        var geo = new GeometryFileDefinition
        {
            GeometryId = "PN.Ab12",
            Units = GeometricUnits.m,
            Model = objParser.Parse(modelPath, NullLogger.Instance),
            FileName = Path.GetFileName(modelPath)
        };
        var luminaire = new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "Example-Tool"
            },
            GeometryDefinitions = [geo],
            Parts =
            [
                new()
                {
                    Name = "luminaire",
                    LightEmittingObjects =
                    [
                        new(new Rectangle
                        {
                            SizeX = 0.5, SizeY = 0.25
                        })
                        {
                            Name = "leo"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "les", FaceAssignments = [new SingleFaceAssignment {FaceIndex = 3}],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leo"] = 1
                            }
                        }
                    ],
                    GeometryReference = geo
                }
            ]
        };
        var written = new Writer().WriteToByteArray(luminaire);
        var read = new Reader().ReadContainer(written);
        luminaire.Should().BeEquivalentTo(read, o => o.WithStrictOrdering());
    }
}