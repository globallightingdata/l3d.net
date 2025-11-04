using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UnusedMethodReturnValue.Local

namespace L3D.Net.Tests.Internal;

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

        ValidateContainerMessage(containerTypeToTest, context, Validation.All).NotBeEmpty();

        context.XmlValidator.Received(1).ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallXmlValidatorValidateFile_WhenFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        ValidateContainerMessage(containerTypeToTest, context, Validation.DoesReferencedObjectsExist).BeEmpty();

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallValidate_WhenCacheIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);

        ValidateContainerMessage(containerTypeToTest, context, Validation.All).NotBeEmpty();

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallValidate_WhenStructureXmlIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        ValidateContainerMessage(containerTypeToTest, context, Validation.All).NotBeEmpty();

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenStructureXmlIsNullAndFlagSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        ValidateContainerMessage(containerTypeToTest, context, Validation.All)
            .ContainSingle(d => d.Message == ErrorMessages.StructureXmlMissing);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenStructureXmlIsNullAndFlagNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());
        ValidateContainerMessage(containerTypeToTest, context, Validation.DoesReferencedObjectsExist)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenCacheIsNullAndFlagSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);
        ValidateContainerMessage(containerTypeToTest, context, Validation.All)
            .ContainSingle(d => d.Message == ErrorMessages.InvalidZip);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenCacheIsNullAndFlagNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);
        ValidateContainerMessage(containerTypeToTest, context, Validation.DoesReferencedObjectsExist)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingReferencesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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
        ValidateContainerMessage(containerTypeToTest, context, Validation.DoesReferencedObjectsExist)
            .Contain(d => d.Message == ErrorMessages.MissingGeometryReference).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingReferencesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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
        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireIsNullAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns((Luminaire) null!);
        ValidateContainerMessage(containerTypeToTest, context, Validation.All)
            .ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireIsNullAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns((Luminaire) null!);

        ValidateContainerMessage(containerTypeToTest, context, Validation.DoesReferencedObjectsExist)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasUnusedFilesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.Files.Returns(new Dictionary<string, FileInformation>());

        var model3 = Substitute.For<IModel3D>();
        model3.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
        {
            ["mtl1"] = []
        });
        model3.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        });
        model3.Files.Returns(new Dictionary<string, FileInformation>());

        var model4 = Substitute.For<IModel3D>();
        model4.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model4.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model4.Files.Returns(new Dictionary<string, FileInformation>());

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

        ValidateContainerMessage(containerTypeToTest, context, Validation.AreAllFileDefinitionsUsed)
            .Contain(d => d.Message == ErrorMessages.UnusedFile).And.HaveCount(3);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasUnusedFilesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model3 = Substitute.For<IModel3D>();
        model3.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingMaterialAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.Files.Returns(new Dictionary<string, FileInformation>
        {
            ["mtl1"] = new() {Status = FileStatus.MissingMaterial}
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
                    }
                }
            ],
            GeometryDefinitions =
            [
                new()
                {
                    GeometryId = "id1",
                    Model = model1
                }
            ]
        });

        ValidateContainerMessage(containerTypeToTest, context, Validation.HasAllMaterials)
            .ContainSingle(d => d.Message == ErrorMessages.MissingMaterial);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingMaterialsAndFlagIsSet_RealExample(ContainerTypeToTest containerTypeToTest)
    {
        var validator = new ContainerValidator(new FileHandler(), new XmlValidator(), new L3DXmlReader());
        var path = Path.Combine(Setup.ValidationDirectory, "example_014.l3d");
        const Validation flags = Validation.HasAllMaterials;

        var validationResult = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => validator.Validate(path, flags),
            ContainerTypeToTest.Bytes => validator.Validate(File.ReadAllBytes(path), flags),
            ContainerTypeToTest.Stream => validator.Validate(new MemoryStream(File.ReadAllBytes(path)), flags),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        validationResult.Should().ContainSingle(d => d.Message == ErrorMessages.MissingMaterial);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingMaterialAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model2 = Substitute.For<IModel3D>();
        model2.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
        {
            ["mtl1"] = []
        });
        model2.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        });

        var model4 = Substitute.For<IModel3D>();
        model4.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingTextureAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.Files.Returns(new Dictionary<string, FileInformation>
        {
            ["tex1"] = new() {Status = FileStatus.MissingTexture}
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
                    }
                }
            ],
            GeometryDefinitions =
            [
                new()
                {
                    GeometryId = "id1",
                    Model = model1
                }
            ]
        });

        ValidateContainerMessage(containerTypeToTest, context, Validation.HasAllTextures)
            .ContainSingle(d => d.Message == ErrorMessages.MissingTexture);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingTextureAndFlagIsSet_RealExample(ContainerTypeToTest containerTypeToTest)
    {
        var validator = new ContainerValidator(new FileHandler(), new XmlValidator(), new L3DXmlReader());
        var path = Path.Combine(Setup.ValidationDirectory, "example_013.l3d");
        const Validation flags = Validation.HasAllTextures;

        var validationResult = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => validator.Validate(path, flags),
            ContainerTypeToTest.Bytes => validator.Validate(File.ReadAllBytes(path), flags),
            ContainerTypeToTest.Stream => validator.Validate(new MemoryStream(File.ReadAllBytes(path)), flags),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        validationResult.Should().ContainSingle(d => d.Message == ErrorMessages.MissingTexture);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingTextureAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        var model2 = Substitute.For<IModel3D>();
        model2.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model2.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        });

        var model4 = Substitute.For<IModel3D>();
        model4.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidCreatedWithApplicationAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? value)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MandatoryField)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidCreatedWithApplicationAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? value)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasDuplicatedPartNamesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.NameConvention)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasDuplicatedPartNamesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoGeometryDefinitionsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MandatoryField)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoGeometryDefinitionsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoPartsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MandatoryField)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoPartsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoLightEmittingPartAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.HasLightEmittingPart)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoLightEmittingPartAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidPartNamesAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.NameConvention)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidPartNamesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidGroupIndexAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidGroupIndexAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.FaceReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexBeginAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexBeginAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexEndAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexEndAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceReferencesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.FaceReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceReferencesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidNameReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.NameReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidNameReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidIntensitiesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidIntensitiesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidAxisRotationsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidAxisRotationsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasJointWithoutGeometriesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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
        ValidateContainerMessage(containerTypeToTest, context, Validation.MandatoryField)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasJointWithoutGeometriesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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
        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidLightEmittingPartShapesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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
        ValidateContainerMessage(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(3);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidLightEmittingPartShapesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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
        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid).BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidGeometryReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.GeometryReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidGeometryReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid).BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoModelAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.GeometryReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoModelAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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

        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid).BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoModelDataAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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
        ValidateContainerMessage(containerTypeToTest, context, Validation.GeometryReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoModelDataAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
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
        ValidateContainerMessage(containerTypeToTest, context, Validation.IsXmlValid).BeEmpty();
    }

    [CustomAssertion]
    private static GenericCollectionAssertions<ValidationHint> ValidateContainerMessage(ContainerTypeToTest containerTypeToTest, Context context, Validation flags)
    {
        var validationResult = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => context.ContainerValidator.Validate(Guid.NewGuid().ToString(), flags),
            ContainerTypeToTest.Bytes => context.ContainerValidator.Validate([0, 1, 2, 3, 4], flags),
            ContainerTypeToTest.Stream => context.ContainerValidator.Validate(new MemoryStream([0, 1, 2, 3, 4]), flags),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        return validationResult.Should();
    }
}