using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using L3D.Net.Abstract;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests.Internal;

[TestFixture]
public partial class ContainerValidatorTests
{
    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void CreateValidationResult_ShouldThrowArgumentException_WhenContainerPathIsNullOrEmpty(string containerPath)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.CreateValidationResult(containerPath, Validation.All);
        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyByteArrayValues))]
    public void CreateValidationResult_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(byte[] containerBytes)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.CreateValidationResult(containerBytes, Validation.All);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(typeof(Setup), nameof(Setup.EmptyStreamValues))]
    public void CreateValidationResult_ShouldThrowArgumentException_WhenContainerBytesIsNullOrEmpty(Stream containerStream)
    {
        var context = CreateContext();

        var action = () => context.ContainerValidator.CreateValidationResult(containerStream, Validation.All);

        action.Should().Throw<ArgumentException>();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldCallFileHandlerExtractContainer(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Path:
                var containerPath = Guid.NewGuid().ToString();
                _ = context.ContainerValidator.CreateValidationResult(containerPath, Validation.All);

                context.FileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerPath));
                break;
            case ContainerTypeToTest.Bytes:
                var containerBytes = new byte[] {0, 1, 2, 3, 4};
                _ = context.ContainerValidator.CreateValidationResult(containerBytes, Validation.All);

                context.FileHandler.Received(1)
                    .ExtractContainer(Arg.Is(containerBytes));
                break;
            case ContainerTypeToTest.Stream:
                using (var ms = new MemoryStream([0, 1, 2, 3, 4]))
                {
                    _ = context.ContainerValidator.CreateValidationResult(ms, Validation.All);

                    context.FileHandler.Received(1)
                        .ExtractContainer(Arg.Is(ms));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldCallXmlValidatorValidateFile_WhenFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.XmlValidator.ValidateStream(Arg.Any<Stream>()).Returns(new List<ValidationHint> {new StructureXmlValidationHint("Test")});

        _ = CreateValidationResultContainer(containerTypeToTest, context, Validation.All);

        context.XmlValidator.Received(1).ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotCallXmlValidatorValidateFile_WhenFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        _ = CreateValidationResultContainer(containerTypeToTest, context, Validation.DoesReferencedObjectsExist);

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotCallValidateStream_WhenCacheIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);

        _ = CreateValidationResultContainer(containerTypeToTest, context, Validation.All);

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotCallValidateStream_WhenStructureXmlIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        _ = CreateValidationResultContainer(containerTypeToTest, context, Validation.All);

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenStructureXmlIsNullAndFlagSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.All);
        result.ValidationHints.Should().ContainSingle(d => d.Message == ErrorMessages.StructureXmlMissing);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenStructureXmlIsNullAndFlagNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());
        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.DoesReferencedObjectsExist);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenCacheIsNullAndFlagSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);
        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.All);
        result.ValidationHints.Should().ContainSingle(d => d.Message == ErrorMessages.InvalidZip);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenCacheIsNullAndFlagNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);
        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.DoesReferencedObjectsExist);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasMissingReferencesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireWithMissingReferences(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.DoesReferencedObjectsExist);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.MissingGeometryReference).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasMissingReferencesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireWithMissingReferences(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireIsNullAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireIsNull(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.All);
        result.ValidationHints.Should().ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireIsNullAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireIsNull(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.DoesReferencedObjectsExist);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasUnusedFilesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasUnusedFiles(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.AreAllFileDefinitionsUsed);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.UnusedFile).And.HaveCount(3);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasUnusedFilesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasUnusedFiles(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasUnusedFilesUsingFilesDictionaryAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasUnusedFilesUsingFilesDictionary(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.AreAllFileDefinitionsUsed);
        result.ValidationHints.Should().ContainSingle(d => d.Message == ErrorMessages.UnusedFile);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasUnusedFilesUsingFilesDictionaryAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasUnusedFilesUsingFilesDictionary(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasMissingMaterialsAndFlagIsSet_RealExample(ContainerTypeToTest containerTypeToTest)
    {
        var validator = new ContainerValidator(new FileHandler(), new XmlValidator(), new L3DXmlReader());
        var path = Path.Combine(Setup.ValidationDirectory, "example_014.l3d");
        const Validation flags = Validation.HasAllMaterials;

        var validationResult = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => validator.CreateValidationResult(path, flags),
            ContainerTypeToTest.Bytes => validator.CreateValidationResult(File.ReadAllBytes(path), flags),
            ContainerTypeToTest.Stream => validator.CreateValidationResult(new MemoryStream(File.ReadAllBytes(path)), flags),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        validationResult.ValidationHints.Should().ContainSingle(d => d.Message == ErrorMessages.MissingMaterial);
        validationResult.Luminaire.Should().NotBeNull();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasMissingMaterialUsingFilesDictionaryAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasMissingMaterialUsingFilesDictionary(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.HasAllMaterials);
        result.ValidationHints.Should().ContainSingle(d => d.Message == ErrorMessages.MissingMaterial);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasMissingMaterialUsingFilesDictionaryAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasMissingMaterialUsingFilesDictionary(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasMissingTextureAndFlagIsSet_RealExample(ContainerTypeToTest containerTypeToTest)
    {
        var validator = new ContainerValidator(new FileHandler(), new XmlValidator(), new L3DXmlReader());
        var path = Path.Combine(Setup.ValidationDirectory, "example_013.l3d");
        const Validation flags = Validation.HasAllTextures;

        var validationResult = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => validator.CreateValidationResult(path, flags),
            ContainerTypeToTest.Bytes => validator.CreateValidationResult(File.ReadAllBytes(path), flags),
            ContainerTypeToTest.Stream => validator.CreateValidationResult(new MemoryStream(File.ReadAllBytes(path)), flags),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        validationResult.ValidationHints.Should().ContainSingle(d => d.Message == ErrorMessages.MissingTexture);
        validationResult.Luminaire.Should().NotBeNull();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasMissingTextureUsingFilesDictionaryAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasMissingTextureUsingFilesDictionary(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.HasAllTextures);
        result.ValidationHints.Should().ContainSingle(d => d.Message == ErrorMessages.MissingTexture);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasMissingTextureUsingFilesDictionaryAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasMissingTextureUsingFilesDictionary(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidCreatedWithApplicationAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? value)
    {
        var context = CreateContext();
        MockLuminaireHasInvalidCreatedWithApplication(context, value);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MandatoryField);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidCreatedWithApplicationAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest,
        string? value)
    {
        var context = CreateContext();
        MockLuminaireHasInvalidCreatedWithApplication(context, value);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasDuplicatedPartNamesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasDuplicatedPartNames(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.NameConvention);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasDuplicatedPartNamesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasDuplicatedPartNames(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasNoGeometryDefinitionsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoGeometryDefinitions(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MandatoryField);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasNoGeometryDefinitionsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoGeometryDefinitions(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasNoPartsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoParts(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MandatoryField);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasNoPartsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoParts(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasNoLightEmittingPartAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoLightEmittingPart(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.HasLightEmittingPart);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasNoLightEmittingPartAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireHasNoLightEmittingPart(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidPartNamesAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidPartNames(context, invalid);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.NameConvention);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidPartNamesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidPartNames(context, invalid);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidGroupIndexAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidGroupIndex(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MinMaxRestriction);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidGroupIndexAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidGroupIndex(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndex(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MinMaxRestriction);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndex(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceReference(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.FaceReferences);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceReference(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexBeginAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndexBegin(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MinMaxRestriction);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexBeginAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndexBegin(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexEndAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndexEnd(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MinMaxRestriction);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexEndAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndexEnd(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceReferencesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceReferences(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.FaceReferences);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceReferencesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceReferences(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidNameReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidNameReference(context, invalid);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.NameReferences);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidNameReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidNameReference(context, invalid);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidIntensitiesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidIntensities(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MinMaxRestriction);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidIntensitiesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidIntensities(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidAxisRotationsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidAxisRotations(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MinMaxRestriction);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidAxisRotationsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidAxisRotations(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasJointWithoutGeometriesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireHasJointWithoutGeometries(context);
        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MandatoryField);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasJointWithoutGeometriesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireHasJointWithoutGeometries(context);
        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidLightEmittingPartShapesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidLightEmittingPartShapes(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.MinMaxRestriction);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(3);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidLightEmittingPartShapesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidLightEmittingPartShapes(context);
        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasInvalidGeometryReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidGeometryReference(context, invalid);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.GeometryReferences);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidGeometryReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest,
        string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidGeometryReference(context, invalid);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasNoModelAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoModel(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.GeometryReferences);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasNoModelAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoModel(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldReturnValidationHint_WhenLuminaireHasNoModelDataAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MocknLuminaireHasNoModelData(context);

        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.GeometryReferences);
        result.ValidationHints.Should().Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void CreateValidationResult_ShouldNotReturnValidationHint_WhenLuminaireHasNoModelDataAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MocknLuminaireHasNoModelData(context);
        var result = CreateValidationResultContainer(containerTypeToTest, context, Validation.IsXmlValid);
        result.ValidationHints.Should().BeEmpty();
    }
}