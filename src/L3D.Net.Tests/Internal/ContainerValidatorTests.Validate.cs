using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        CreateValidateAssertion(containerTypeToTest, context, Validation.All).NotBeEmpty();

        context.XmlValidator.Received(1).ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallXmlValidatorValidateFile_WhenFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        CreateValidateAssertion(containerTypeToTest, context, Validation.DoesReferencedObjectsExist).BeEmpty();

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallValidate_WhenCacheIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);

        CreateValidateAssertion(containerTypeToTest, context, Validation.All).NotBeEmpty();

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotCallValidate_WhenStructureXmlIsNull(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        CreateValidateAssertion(containerTypeToTest, context, Validation.All).NotBeEmpty();

        context.XmlValidator.DidNotReceive().ValidateStream(Arg.Any<Stream>());
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenStructureXmlIsNullAndFlagSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());

        CreateValidateAssertion(containerTypeToTest, context, Validation.All)
            .ContainSingle(d => d.Message == ErrorMessages.StructureXmlMissing);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenStructureXmlIsNullAndFlagNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns(new ContainerCache());
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns(new ContainerCache());
        CreateValidateAssertion(containerTypeToTest, context, Validation.DoesReferencedObjectsExist)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenCacheIsNullAndFlagSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);
        CreateValidateAssertion(containerTypeToTest, context, Validation.All)
            .ContainSingle(d => d.Message == ErrorMessages.InvalidZip);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenCacheIsNullAndFlagNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        context.FileHandler.ExtractContainer(Arg.Any<Stream>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<byte[]>()).Returns((ContainerCache) null!);
        context.FileHandler.ExtractContainer(Arg.Any<string>()).Returns((ContainerCache) null!);
        CreateValidateAssertion(containerTypeToTest, context, Validation.DoesReferencedObjectsExist)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingReferencesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireWithMissingReferences(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.DoesReferencedObjectsExist)
            .Contain(d => d.Message == ErrorMessages.MissingGeometryReference).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingReferencesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireWithMissingReferences(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireIsNullAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireIsNull(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.All)
            .ContainSingle(d => d.Message == ErrorMessages.NotAL3D);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireIsNullAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireIsNull(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.DoesReferencedObjectsExist)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasUnusedFilesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasUnusedFiles(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.AreAllFileDefinitionsUsed)
            .Contain(d => d.Message == ErrorMessages.UnusedFile).And.HaveCount(3);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasUnusedFilesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasUnusedFiles(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasUnusedFilesUsingFilesDictionaryAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasUnusedFilesUsingFilesDictionary(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.AreAllFileDefinitionsUsed)
            .ContainSingle(d => d.Message == ErrorMessages.UnusedFile);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasUnusedFilesUsingFilesDictionaryAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasUnusedFilesUsingFilesDictionary(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
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
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingMaterialUsingFilesDictionaryAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasMissingMaterialUsingFilesDictionary(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.HasAllMaterials)
            .ContainSingle(d => d.Message == ErrorMessages.MissingMaterial);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingMaterialUsingFilesDictionaryAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasMissingMaterialUsingFilesDictionary(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
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
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasMissingTextureUsingFilesDictionaryAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasMissingTextureUsingFilesDictionary(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.HasAllTextures)
            .ContainSingle(d => d.Message == ErrorMessages.MissingTexture);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasMissingTextureUsingFilesDictionaryAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasMissingTextureUsingFilesDictionary(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidCreatedWithApplicationAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? value)
    {
        var context = CreateContext();
        MockLuminaireHasInvalidCreatedWithApplication(context, value);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MandatoryField)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidCreatedWithApplicationAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? value)
    {
        var context = CreateContext();
        MockLuminaireHasInvalidCreatedWithApplication(context, value);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasDuplicatedPartNamesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasDuplicatedPartNames(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.NameConvention)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasDuplicatedPartNamesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasDuplicatedPartNames(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoGeometryDefinitionsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoGeometryDefinitions(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MandatoryField)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoGeometryDefinitionsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoGeometryDefinitions(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoPartsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoParts(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MandatoryField)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoPartsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoParts(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoLightEmittingPartAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoLightEmittingPart(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.HasLightEmittingPart)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoLightEmittingPartAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireHasNoLightEmittingPart(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidPartNamesAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidPartNames(context, invalid);

        CreateValidateAssertion(containerTypeToTest, context, Validation.NameConvention)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(6);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidPartNamesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidPartNames(context, invalid);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidGroupIndexAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidGroupIndex(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidGroupIndexAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidGroupIndex(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndex(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndex(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceReference(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.FaceReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceReference(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexBeginAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndexBegin(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexBeginAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndexBegin(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceIndexEndAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndexEnd(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceIndexEndAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceIndexEnd(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidFaceReferencesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceReferences(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.FaceReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidFaceReferencesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidFaceReferences(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidNameReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidNameReference(context, invalid);

        CreateValidateAssertion(containerTypeToTest, context, Validation.NameReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidNameReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidNameReference(context, invalid);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidIntensitiesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidIntensities(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidIntensitiesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidIntensities(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidAxisRotationsAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidAxisRotations(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(5);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidAxisRotationsAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidAxisRotations(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasJointWithoutGeometriesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireHasJointWithoutGeometries(context);
        CreateValidateAssertion(containerTypeToTest, context, Validation.MandatoryField)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasJointWithoutGeometriesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MockLuminaireHasJointWithoutGeometries(context);
        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid)
            .BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidLightEmittingPartShapesAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidLightEmittingPartShapes(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.MinMaxRestriction)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(3);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidLightEmittingPartShapesAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidLightEmittingPartShapes(context);
        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid).BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasInvalidGeometryReferenceAndFlagIsSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidGeometryReference(context, invalid);

        CreateValidateAssertion(containerTypeToTest, context, Validation.GeometryReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValuesAndEmptyStrings))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasInvalidGeometryReferenceAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest, string? invalid)
    {
        var context = CreateContext();

        MockLuminaireHasInvalidGeometryReference(context, invalid);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid).BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoModelAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoModel(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.GeometryReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(1);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoModelAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();

        MockLuminaireHasNoModel(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid).BeEmpty();
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldReturnValidationHint_WhenLuminaireHasNoModelDataAndFlagIsSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MocknLuminaireHasNoModelData(context);

        CreateValidateAssertion(containerTypeToTest, context, Validation.GeometryReferences)
            .Contain(d => d.Message == ErrorMessages.InvalidL3DContent).And.HaveCount(2);
    }

    [Test, TestCaseSource(nameof(ContainerTypeToTestEnumValues))]
    public void Validate_ShouldNotReturnValidationHint_WhenLuminaireHasNoModelDataAndFlagIsNotSet(ContainerTypeToTest containerTypeToTest)
    {
        var context = CreateContext();
        MocknLuminaireHasNoModelData(context);
        CreateValidateAssertion(containerTypeToTest, context, Validation.IsXmlValid).BeEmpty();
    }
}