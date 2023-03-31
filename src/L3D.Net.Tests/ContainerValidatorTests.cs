using FluentAssertions;
using L3D.Net.Abstract;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using L3D.Net.Data;
using L3D.Net.XML;

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
        public IL3DXmlReader L3DXmlReader { get; }
        public ContainerValidator ContainerValidator { get; }

        public Context()
        {
            FileHandler = Substitute.For<IFileHandler>();
            XmlValidator = Substitute.For<IXmlValidator>();
            L3DXmlReader = Substitute.For<IL3DXmlReader>();
            ContainerValidator = new ContainerValidator(FileHandler, XmlValidator, L3DXmlReader);
            FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache { StructureXml = Stream.Null });
            FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache { StructureXml = Stream.Null });
            FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache { StructureXml = Stream.Null });
            L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire());
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

    private static IEnumerable<ContainerTypeToTest> ContainerTypeToTestEnumValues => Enum.GetValues<ContainerTypeToTest>();

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileHandlerIsNull()
    {
        var action = () => _ = new ContainerValidator(null!,
            Substitute.For<IXmlValidator>(),
            Substitute.For<IL3DXmlReader>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenXmlValidatorIsNull()
    {
        var action = () => _ = new ContainerValidator(Substitute.For<IFileHandler>(),
            null!,
            Substitute.For<IL3DXmlReader>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenL3DXmlReaderIsNull()
    {
        var action = () => _ = new ContainerValidator(Substitute.For<IFileHandler>(),
            Substitute.For<IXmlValidator>(),
            null!
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string containerPath)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.Validate(containerPath, Validation.All);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyByteArrayValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(byte[] containerBytes)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.Validate(containerBytes, Validation.All);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStreamValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(Stream containerStream)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.Validate(containerStream, Validation.All);

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
                context.ContainerValidator.Validate(containerPath, Validation.All);

                context.FileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerPath));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] { 0, 1, 2, 3, 4 };
                context.ContainerValidator.Validate(containerBytes, Validation.All);

                context.FileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerBytes));
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms, Validation.All);

                    context.FileHandler.Received(1)
                        .ExtractContainer(Arg.Is(ms));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldCallXmlValidatorValidateFile_WhenFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.XmlValidator.ValidateStream(Arg.Any<Stream>()).Returns(new List<ValidationHint> { new StructureXmlValidationHint("Test") });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _ = context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Bytes:
                _ = context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    _ = context.ContainerValidator.Validate(ms, Validation.All).ToArray();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.XmlValidator.Received(1).ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallXmlValidatorValidateFile_WhenFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _ = context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.DoesReferencedObjectsExist).ToArray();
                break;
            case ContainerTypeToTest.Bytes:
                _ = context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.DoesReferencedObjectsExist).ToArray();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    _ = context.ContainerValidator.Validate(ms, Validation.DoesReferencedObjectsExist).ToArray();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallValidate_WhenCacheIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache)null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache)null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache)null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _ = context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Bytes:
                _ = context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    _ = context.ContainerValidator.Validate(ms, Validation.All).ToArray();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallValidate_WhenStructureXmlIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _ = context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Bytes:
                _ = context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    _ = context.ContainerValidator.Validate(ms, Validation.All).ToArray();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenStructureXmlIsNullAndFlagSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlMissing);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlMissing);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms, Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlMissing);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenStructureXmlIsNullAndFlagNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms, Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenCacheIsNullAndFlagSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache)null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache)null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache)null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.InvalidZip);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.InvalidZip);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms, Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.InvalidZip);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenCacheIsNullAndFlagNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache)null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache)null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache)null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms, Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingReferencesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts = new List<GeometryPart>
            {
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints = new List<JointPart>
                    {
                        new()
                        {
                            Geometries = new List<GeometryPart>
                            {
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2"
                                    }
                                },
                                new()
                            }
                        }
                    }
                }
            }
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).Should()
                    .Contain(d => d.Message == ErrorMessages.MissingGeometryReference).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.All).Should()
                    .Contain(d => d.Message == ErrorMessages.MissingGeometryReference).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms, Validation.All).Should()
                        .Contain(d => d.Message == ErrorMessages.MissingGeometryReference).And.HaveCount(2);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingReferencesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts = new List<GeometryPart>
            {
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints = new List<JointPart>
                    {
                        new()
                        {
                            Geometries = new List<GeometryPart>
                            {
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2"
                                    }
                                },
                                new()
                            }
                        }
                    }
                }
            }
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns((Luminaire)null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).Should()
                    .ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate(new byte[] { 0, 1, 2, 3, 4 }, Validation.All).Should()
                    .ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream(new byte[] { 0, 1, 2, 3, 4 }))
                {
                    context.ContainerValidator.Validate(ms, Validation.All).Should()
                        .ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }
}