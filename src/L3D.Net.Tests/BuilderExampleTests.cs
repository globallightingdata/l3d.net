using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Extensions.Logging.NSubstitute;
using FluentAssertions;
using FluentAssertions.Equivalency;
using L3D.Net.Geometry;
using L3D.Net.Internal;
using L3D.Net.XML;
using L3D.Net.XML.V0_9_2;
using L3D.Net.XML.V0_9_2.Dto;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace L3D.Net.Tests;

[TestFixture]
public class BuilderExampleTests
{
    private readonly List<string> _tempDirectories = new();
    private ContainerBuilder _containerBuilder;
    private IXmlDtoSerializer _xmlDtoSerializer;
    private LuminaireBuilder _builder;

    static List<string> ExampleDirectories()
    {
        Setup.Initialize();
        var directories = Directory.EnumerateDirectories(Setup.ExamplesDirectory).ToList();
        return directories;
    }
        
    private string GetTempDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "test.gldf.io", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);
        _tempDirectories.Add(tempDirectory);
        return tempDirectory;
    }
        
    [SetUp]
    public void Init()
    {
        Setup.Initialize();
        var fileHandler = new FileHandler();
        var xmlDtoConverter = new XmlDtoConverter();
        _xmlDtoSerializer = new XmlDtoSerializer();
        var xmlValidator = new XmlValidator();
        var objParser = new ObjParser();
        var logger = LoggerSubstitute.Create();

        _containerBuilder = new ContainerBuilder(
            fileHandler,
            xmlDtoConverter,
            _xmlDtoSerializer,
            xmlValidator,
            logger
        );

        _builder = new LuminaireBuilder("BuilderExampleTests", objParser, _containerBuilder, logger);
    }

    [TearDown]
    public void Deinit()
    {
        foreach (var tempDirectory in _tempDirectories)
        {
            try
            {
                Directory.Delete(tempDirectory, true);
            }
            catch
            {
                // ignore
            }
        }
    }

    private EquivalencyAssertionOptions<LuminaireDto> ExampleDtoOptions(
        EquivalencyAssertionOptions<LuminaireDto> options)
    {
        return options
            .IncludingAllRuntimeProperties()
            .AllowingInfiniteRecursion()
            .Excluding(dto => dto.Header.CreationTimeCode)
            .Excluding(ctx => ctx.Path.EndsWith("Id"))
            .Excluding(ctx => ctx.Path.EndsWith("GeometryId"));
    }

    [Test]
    [TestCaseSource(nameof(ExampleDirectories))]
    public void ExampleDtoTests(string exampleDirectory)
    {
        var tempDirectory = GetTempDirectory();
        var exampleName = Path.GetFileName(exampleDirectory).ToLower();

        if (!Setup.ExampleBuilderMapping.TryGetValue(exampleName, out var testFunction))
            throw new Exception($"No test code for example '{exampleName}' available!");

        var xmlFilePath = Path.Combine(tempDirectory, Constants.L3dXmlFilename);
        var xmlExampleFilePath = Path.Combine(exampleDirectory, Constants.L3dXmlFilename);

        _builder = testFunction(_builder);

        _containerBuilder.PrepareFiles(_builder.Luminaire, tempDirectory);

        var luminaireDto = new XmlDtoConverter().Convert(_builder.Luminaire);
        var readDto = new XmlDtoSerializer().Deserialize(xmlFilePath);
        var readExampleDto = new XmlDtoSerializer().Deserialize(xmlExampleFilePath);

        readDto.Should().BeEquivalentTo(luminaireDto, ExampleDtoOptions);
        luminaireDto.Should().BeEquivalentTo(readExampleDto, ExampleDtoOptions);
    }

    [Test]
    [TestCaseSource(nameof(ExampleDirectories))]
    public void BuildContainerFilesTests(string exampleDirectory)
    {
        var tempDirectory = GetTempDirectory();
        var exampleName = Path.GetFileName(exampleDirectory).ToLower();

        if (!Setup.ExampleBuilderMapping.TryGetValue(exampleName, out var testFunction))
            throw new Exception($"No test code for example '{exampleName}' available!");

        _builder = testFunction(_builder);

        _containerBuilder.PrepareFiles(_builder.Luminaire, tempDirectory);

        _builder.Luminaire.GeometryDefinitions.Should().NotBeEmpty();

        CheckContainerFiles(tempDirectory);
    }

    private void CheckContainerFiles(string tempDirectory)
    {
        File.Exists(Path.Combine(tempDirectory, Constants.L3dXmlFilename)).Should().BeTrue();

        foreach (var geometryDefinition in _builder.Luminaire.GeometryDefinitions)
        {
            var modelFileName = Path.GetFileName(geometryDefinition.Model.FilePath);
            var expectedModelFilePath = Path.Combine(tempDirectory, geometryDefinition.Id, modelFileName!);
            File.Exists(expectedModelFilePath).Should().BeTrue();

            foreach (var referencedMaterialFile in geometryDefinition.Model.ReferencedMaterialLibraryFiles)
            {
                var materialFileName = Path.GetFileName(referencedMaterialFile);
                var expectedMaterialFilePath = Path.Combine(tempDirectory, geometryDefinition.Id, materialFileName);
                File.Exists(expectedMaterialFilePath).Should().BeTrue();
            }

            foreach (var referencedTextureFile in geometryDefinition.Model.ReferencedTextureFiles)
            {
                var textureFileName = Path.GetFileName(referencedTextureFile);
                var expectedTextureFilePath = Path.Combine(tempDirectory, geometryDefinition.Id, textureFileName);
                File.Exists(expectedTextureFilePath).Should().BeTrue();
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(ExampleDirectories))]
    public void BuildContainerTests(string exampleDirectory)
    {
        var containerTempDirectory = GetTempDirectory();
        var testTempDirectory = GetTempDirectory();

        var exampleName = Path.GetFileName(exampleDirectory).ToLower();

        if (!Setup.ExampleBuilderMapping.TryGetValue(exampleName, out var testFunction))
            throw new Exception($"No test code for example '{exampleName}' available!");

        _builder = testFunction(_builder);

        var containerPath = Path.Combine(containerTempDirectory, "luminaire" + Constants.L3dExtension);
        _builder.Build(containerPath);

        File.Exists(containerPath).Should().BeTrue();

        ZipFile.ExtractToDirectory(containerPath, testTempDirectory);

        _builder.Luminaire.GeometryDefinitions.Should().NotBeEmpty();

        CheckContainerFiles(testTempDirectory);
    }

    [Test]
    [TestCaseSource(nameof(ExampleDirectories))]
    public void BuildFromDtoTests(string exampleDirectory)
    {
        var exampleName = Path.GetFileName(exampleDirectory).ToLower();

        if (!Setup.ExampleBuilderMapping.TryGetValue(exampleName, out var testFunction))
            throw new Exception($"No test code for example '{exampleName}' available!");

        _builder = testFunction(_builder);

        var expectedLuminaire = _builder.Luminaire;
            
        var luminaireDto = _xmlDtoSerializer.Deserialize(Path.Combine(exampleDirectory, Constants.L3dXmlFilename));

        var fromContainerBuilder = Builder.NewLuminaire(Substitute.For<ILogger>());

        var luminaireFromDtoBuilder = new LuminaireFromDtoConstructor();

        var luminaire = luminaireFromDtoBuilder.BuildLuminaireFromDto(fromContainerBuilder, luminaireDto, exampleDirectory);

        luminaire.Should().BeEquivalentTo(expectedLuminaire, options => options
            .IncludingAllRuntimeProperties()
            .AllowingInfiniteRecursion()
            .Excluding(dto => dto.Header.CreationTimeCode));
    }
}