using FluentAssertions;
using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache {StructureXml = Stream.Null});
            FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache {StructureXml = Stream.Null});
            FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache {StructureXml = Stream.Null});
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

    private static IEnumerable<string?> EmptyStringValues => [null, "", string.Empty, " ", "    "];

    private static IEnumerable<TestCaseData> ContainerTypeToTestEnumValuesAndEmptyStrings()
    {
        foreach (var emptyString in EmptyStringValues)
        {
            foreach (var containerTypeToTest in Enum.GetValues<ContainerTypeToTest>())
            {
                yield return new TestCaseData(containerTypeToTest, emptyString);
            }
        }
    }

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

        var action = () => context.ContainerValidator.Validate(containerPath, Validation.All).ToArray();
        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyByteArrayValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(byte[] containerBytes)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.Validate(containerBytes, Validation.All).ToArray();

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStreamValues))]
    public void Validate_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(Stream containerStream)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.Validate(containerStream, Validation.All).ToArray();

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
                _ = context.ContainerValidator.Validate(containerPath, Validation.All).ToArray();

                context.FileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerPath));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] {0, 1, 2, 3, 4};
                _ = context.ContainerValidator.Validate(containerBytes, Validation.All).ToArray();

                context.FileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerBytes));
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    _ = context.ContainerValidator.Validate(ms, Validation.All).ToArray();

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

        context.XmlValidator.ValidateStream(Arg.Any<Stream>()).Returns(new List<ValidationHint> {new StructureXmlValidationHint("Test")});

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _ = context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Bytes:
                _ = context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
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
                _ = context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.DoesReferencedObjectsExist).ToArray();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
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

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                _ = context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Bytes:
                _ = context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
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
                _ = context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.All).ToArray();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
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
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlMissing);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
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
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
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

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.InvalidZip);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.All).Should().ContainSingle(d => d.Message == ErrorMessages.InvalidZip);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
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

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.DoesReferencedObjectsExist).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
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
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new()
                        {
                            Geometries =
                            [
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2"
                                    }
                                },

                                new()
                            ]
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.DoesReferencedObjectsExist).Should()
                    .Contain(d => d.Message == ErrorMessages.MissingGeometryReference).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.DoesReferencedObjectsExist).Should()
                    .Contain(d => d.Message == ErrorMessages.MissingGeometryReference).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.DoesReferencedObjectsExist).Should()
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
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new()
                        {
                            Geometries =
                            [
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2"
                                    }
                                },

                                new()
                            ]
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireIsNullAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns((Luminaire) null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.All).Should()
                    .ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.All).Should()
                    .ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.All).Should()
                        .ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireIsNullAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns((Luminaire) null!);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.DoesReferencedObjectsExist).Should()
                    .BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.DoesReferencedObjectsExist).Should()
                    .BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.DoesReferencedObjectsExist).Should()
                        .BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasUnusedFilesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model3 = Substitute.For<IModel3D?>();
        model3!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
        {
            ["mtl1"] = []
        });
        model3.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        });

        var model4 = Substitute.For<IModel3D?>();
        model4!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model4.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new()
                        {
                            Geometries =
                            [
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2"
                                    }
                                },

                                new()
                            ]
                        }
                    ]
                }
            ],
            GeometryDefinitions =
            [
                new()
                {
                    GeometryId = "id1",
                    Model = model1
                },

                new()
                {
                    GeometryId = "id3",
                    Model = model3
                },

                new()
                {
                    GeometryId = "id4",
                    Model = model4,
                    FileName = "obj1"
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.AreAllFileDefinitionsUsed).Should()
                    .Contain(d => d.Message == ErrorMessages.UnusedFile).And.HaveCount(3);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.AreAllFileDefinitionsUsed).Should()
                    .Contain(d => d.Message == ErrorMessages.UnusedFile).And.HaveCount(3);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.AreAllFileDefinitionsUsed).Should()
                        .Contain(d => d.Message == ErrorMessages.UnusedFile).And.HaveCount(3);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasUnusedFilesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model3 = Substitute.For<IModel3D?>();
        model3!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
        {
            ["mtl1"] = []
        });
        model3.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        });

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new()
                        {
                            Geometries =
                            [
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2"
                                    }
                                },

                                new()
                            ]
                        }
                    ]
                }
            ],
            GeometryDefinitions =
            [
                new()
                {
                    GeometryId = "id1",
                    Model = model1
                },

                new()
                {
                    GeometryId = "id3",
                    Model = model3
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingMaterialAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model2 = Substitute.For<IModel3D?>();
        model2!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
        {
            ["mtl1"] = []
        }, new Dictionary<string, byte[]>());
        model2.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        }, new Dictionary<string, byte[]>());

        var model4 = Substitute.For<IModel3D?>();
        model4!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model4.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new()
                        {
                            Geometries =
                            [
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2",
                                        Model = model2
                                    }
                                },

                                new()
                            ]
                        }
                    ]
                }
            ],
            GeometryDefinitions =
            [
                new()
                {
                    GeometryId = "id1",
                    Model = model1
                },

                new()
                {
                    GeometryId = "id2",
                    Model = model2
                },

                new()
                {
                    GeometryId = "id4",
                    Model = model4,
                    FileName = "obj1"
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.HasAllMaterials).Should()
                    .ContainSingle(d => d.Message == ErrorMessages.MissingMaterial);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.HasAllMaterials).Should()
                    .ContainSingle(d => d.Message == ErrorMessages.MissingMaterial);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.HasAllMaterials).Should()
                        .ContainSingle(d => d.Message == ErrorMessages.MissingMaterial);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingMaterialAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model2 = Substitute.For<IModel3D?>();
        model2!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
        {
            ["mtl1"] = []
        });
        model2.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        });

        var model4 = Substitute.For<IModel3D?>();
        model4!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model4.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new()
                        {
                            Geometries =
                            [
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2",
                                        Model = model2
                                    }
                                },

                                new()
                            ]
                        }
                    ]
                }
            ],
            GeometryDefinitions =
            [
                new()
                {
                    GeometryId = "id1",
                    Model = model1
                },

                new()
                {
                    GeometryId = "id2",
                    Model = model2
                },

                new()
                {
                    GeometryId = "id4",
                    Model = model4,
                    FileName = "obj1"
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingTextureAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model2 = Substitute.For<IModel3D?>();
        model2!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
        {
            ["mtl1"] = []
        }, new Dictionary<string, byte[]>());
        model2.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        }, new Dictionary<string, byte[]>());

        var model4 = Substitute.For<IModel3D?>();
        model4!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model4.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new()
                        {
                            Geometries =
                            [
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2",
                                        Model = model2
                                    }
                                },

                                new()
                            ]
                        }
                    ]
                }
            ],
            GeometryDefinitions =
            [
                new()
                {
                    GeometryId = "id1",
                    Model = model1
                },

                new()
                {
                    GeometryId = "id2",
                    Model = model2
                },

                new()
                {
                    GeometryId = "id4",
                    Model = model4,
                    FileName = "obj1"
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.HasAllTextures).Should()
                    .ContainSingle(d => d.Message == ErrorMessages.MissingTexture);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.HasAllTextures).Should()
                    .ContainSingle(d => d.Message == ErrorMessages.MissingTexture);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.HasAllTextures).Should()
                        .ContainSingle(d => d.Message == ErrorMessages.MissingTexture);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingTextureAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model2 = Substitute.For<IModel3D?>();
        model2!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model2.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        });

        var model4 = Substitute.For<IModel3D?>();
        model4!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model4.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new()
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new()
                        {
                            Geometries =
                            [
                                new()
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2",
                                        Model = model2
                                    }
                                },

                                new()
                            ]
                        }
                    ]
                }
            ],
            GeometryDefinitions =
            [
                new()
                {
                    GeometryId = "id1",
                    Model = model1
                },

                new()
                {
                    GeometryId = "id2",
                    Model = model2
                },

                new()
                {
                    GeometryId = "id4",
                    Model = model4,
                    FileName = "obj1"
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidCreatedWithApplicationAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? value)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = value!
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MandatoryField).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MandatoryField).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MandatoryField).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidCreatedWithApplicationAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? value)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = value!
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasDuplicatedPartNamesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "leoPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.NameConvention).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.NameConvention).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.NameConvention).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasDuplicatedPartNamesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "leoPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoGeometryDefinitionsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MandatoryField).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MandatoryField).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MandatoryField).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoGeometryDefinitionsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoPartsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MandatoryField).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MandatoryField).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MandatoryField).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoPartsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoLightEmittingPartAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    }
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.HasLightEmittingPart).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.HasLightEmittingPart).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.HasLightEmittingPart).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoLightEmittingPartAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    }
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidPartNamesAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "1geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "le"
                        },

                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = invalid!
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "leoPart%",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["le"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new()
                        {
                            Name = "j"
                        }
                    ],
                    Sensors =
                    [
                        new()
                        {
                            Name = "%%%"
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.NameConvention).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.NameConvention).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.NameConvention).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidPartNamesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "1geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "le"
                        },

                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = invalid!
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "leoPart%",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["le"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new()
                        {
                            Name = "j"
                        }
                    ],
                    Sensors =
                    [
                        new()
                        {
                            Name = "%%%"
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidGroupIndexAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = -1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MinMaxRestriction).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidGroupIndexAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = -1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = -1
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MinMaxRestriction).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = -1
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 2,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.FaceReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.FaceReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.FaceReferences).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 2,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexBeginAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new FaceRangeAssignment()
                                {
                                    GroupIndex = 1,
                                    FaceIndexBegin = -1
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MinMaxRestriction).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexBeginAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new FaceRangeAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndexBegin = -1
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexEndAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new FaceRangeAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndexBegin = 2,
                                    FaceIndexEnd = 1
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MinMaxRestriction).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexEndAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceReferencesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new FaceRangeAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndexBegin = 2,
                                    FaceIndexEnd = 3
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.FaceReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.FaceReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.FaceReferences).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceReferencesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new FaceRangeAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndexBegin = 2,
                                    FaceIndexEnd = 3
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidNameReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                [invalid ?? string.Empty] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.NameReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.NameReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.NameReferences).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidNameReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                [invalid ?? string.Empty] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidIntensitiesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart1",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = -0.1
                            }
                        },

                        new()
                        {
                            Name = "lesPart2",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 1.1
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MinMaxRestriction).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidIntensitiesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart1",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = -0.1
                            }
                        },

                        new()
                        {
                            Name = "lesPart2",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 1.1
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidAxisRotationsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new()
                        {
                            Name = "joint",
                            XAxis = new AxisRotation
                            {
                                Max = 0,
                                Min = 0,
                                Step = 0,
                            },
                            YAxis = new AxisRotation
                            {
                                Max = 1,
                                Min = 2,
                                Step = -1,
                            },
                            ZAxis = new AxisRotation
                            {
                                Max = -2,
                                Min = -1,
                                Step = -2,
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MinMaxRestriction).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidAxisRotationsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new()
                        {
                            Name = "joint",
                            XAxis = new AxisRotation
                            {
                                Max = 0,
                                Min = 0,
                                Step = 0,
                            },
                            YAxis = new AxisRotation
                            {
                                Max = 1,
                                Min = 2,
                                Step = -1,
                            },
                            ZAxis = new AxisRotation
                            {
                                Max = -2,
                                Min = -1,
                                Step = -2,
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasJointWithoutGeometriesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new()
                        {
                            Name = "joint"
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MandatoryField).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MandatoryField).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MandatoryField).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasJointWithoutGeometriesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new()
                        {
                            Name = "joint"
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidLightEmittingPartShapesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = -0.1
                        })
                        {
                            Name = "leoPart1"
                        },

                        new(new Rectangle
                        {
                            SizeX = -0.1,
                            SizeY = -0.1
                        })
                        {
                            Name = "leoPart2"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(3);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.MinMaxRestriction).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(3);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.MinMaxRestriction).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(3);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidLightEmittingPartShapesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = -0.1
                        })
                        {
                            Name = "leoPart1"
                        },

                        new(new Rectangle
                        {
                            SizeX = -0.1,
                            SizeY = -0.1
                        })
                        {
                            Name = "leoPart2"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidGeometryReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = invalid!,
                        FileName = invalid!,
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.GeometryReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.GeometryReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.GeometryReferences).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidGeometryReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = invalid!,
                        FileName = invalid!,
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoModelAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition()
                    {
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.GeometryReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.GeometryReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.GeometryReferences).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoModelAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition()
                    {
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoModelDataAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition()
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.GeometryReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.GeometryReferences).Should()
                    .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.GeometryReferences).Should()
                        .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoModelDataAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D?>();
        model1!.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new()
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new()
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new()
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition()
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new()
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                context.ContainerValidator.Validate(Guid.NewGuid().ToString(), Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Bytes:
                context.ContainerValidator.Validate([0, 1, 2, 3, 4], Validation.IsXmlValid).Should().BeEmpty();
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    context.ContainerValidator.Validate(ms, Validation.IsXmlValid).Should().BeEmpty();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }
}