using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using FluentAssertions.Collections;
using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests.Internal;

[TestFixture]
public partial class ContainerValidatorTests
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

    private static IEnumerable<TestCaseData> ContainerTypeToTestEnumValuesAndEmptyStrings()
    {
        foreach (var containerTypeToTest in Enum.GetValues<ContainerTypeToTest>())
        {
            yield return new TestCaseData(containerTypeToTest, null).SetArgDisplayNames(containerTypeToTest.ToString("G"), "<null>");
            yield return new TestCaseData(containerTypeToTest, string.Empty).SetArgDisplayNames(containerTypeToTest.ToString("G"), "");
            yield return new TestCaseData(containerTypeToTest, " ").SetArgDisplayNames(containerTypeToTest.ToString("G"), " ");
            yield return new TestCaseData(containerTypeToTest, "    ").SetArgDisplayNames(containerTypeToTest.ToString("G"), "    ");
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

    [CustomAssertion]
    private static GenericCollectionAssertions<ValidationHint> CreateValidateAssertion(ContainerTypeToTest containerTypeToTest, Context context, Validation flags)
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

    [CustomAssertion]
    private static ValidationResultContainer CreateValidationResultContainer(ContainerTypeToTest containerTypeToTest, Context context, Validation flags)
    {
        var validationResult = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => context.ContainerValidator.CreateValidationResult(Guid.NewGuid().ToString(), flags),
            ContainerTypeToTest.Bytes => context.ContainerValidator.CreateValidationResult([0, 1, 2, 3, 4], flags),
            ContainerTypeToTest.Stream => context.ContainerValidator.CreateValidationResult(new MemoryStream([0, 1, 2, 3, 4]), flags),
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        return validationResult;
    }
}