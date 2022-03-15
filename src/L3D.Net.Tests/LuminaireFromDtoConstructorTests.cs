using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using FluentAssertions.Equivalency;
using L3D.Net.Data;
using L3D.Net.XML.V0_9_1;
using L3D.Net.XML.V0_9_1.Dto;
using NUnit.Framework;

namespace L3D.Net.Tests
{
    [TestFixture]
    public class LuminaireFromDtoConstructorTests
    {
        static List<string> ExampleDirectories()
        {
            Setup.Initialize();
            var directories = Directory.EnumerateDirectories(Setup.ExamplesDirectory).ToList();
            return directories;
        }

        private EquivalencyAssertionOptions<Luminaire> ExampleDtoOptions(
            EquivalencyAssertionOptions<Luminaire> options)
        {
            return options
                .IncludingAllRuntimeProperties()
                .AllowingInfiniteRecursion()
                .Excluding(dto => dto.Header.CreationTimeCode);
        }

        private void CreateExample000TestData(out LuminaireDto luminaireDto, out string directory)
        {
            luminaireDto = new XmlDtoConverter().Convert(Builder.NewLuminaire().BuildExample000().Luminaire);
            luminaireDto.GeometryDefinitions.First().Id = "cube";
            ((GeometryReferenceDto)luminaireDto.Parts.First().GeometrySource).GeometryId = "cube";
            directory = Path.Combine(Setup.ExamplesDirectory, "example_000");
        }
        
        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentNullException_WhenBuilderIsNull()
        {
            var luminaireDto = new LuminaireDto();
            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(null, luminaireDto, Guid.NewGuid().ToString());
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentNullException_WhenLuminaireDtoIsNull()
        {
            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), null, Guid.NewGuid().ToString());
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenDataPathIsNullEmptyOrWhitesapce(string path)
        {
            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), new LuminaireDto(), path);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenLuminaireHasNoMetaData()
        {
            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), new LuminaireDto(), Guid.NewGuid().ToString());
            action.Should().Throw<ArgumentException>().WithMessage(@"Luminaire must contain MetaData (Parameter '*')");
        }
        
        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenGeometryDefinitionListIsNull()
        {
            var luminaireDto = new LuminaireDto
            {
                Header = new HeaderDto
                {
                    CreatedWithApplication = Guid.NewGuid().ToString()
                }
            };
            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, Guid.NewGuid().ToString());
            action.Should().Throw<ArgumentException>().WithMessage(@"Luminaire must contain a GeometryDefinition list (Parameter '*')");
        }
        
        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenPartListIsNull()
        {
            var luminaireDto = new LuminaireDto
            {
                Header = new HeaderDto
                {
                    CreatedWithApplication = Guid.NewGuid().ToString()
                },
                GeometryDefinitions = new List<GeometryDefinitionDto>()
            };
            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, Guid.NewGuid().ToString());
            action.Should().Throw<ArgumentException>().WithMessage(@"Luminaire must contain a Structure list (Parameter '*')");
            
        }

        private class InvalidShapeDto : ShapeDto
        {

        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenLightEmittingObjectHasInvalidShape()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.Parts.First().LightEmittingObjects.First().Shape = new InvalidShapeDto();

            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            action.Should().Throw<Exception>().WithMessage(@"Invalid Shape type in LightEmittingNodeDto: *");
        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenLightEmittingObjectShapeIsNull()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.Parts.First().LightEmittingObjects.First().Shape = null;

            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            action.Should().Throw<Exception>().WithMessage(@"Invalid Shape type in LightEmittingNodeDto: *");
        }

        private class InvalidFaceAssignmentDto : FaceAssignmentBaseDto
        {
            public InvalidFaceAssignmentDto()
            {
                GroupIndex = 0;
            }
        };

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenLightEmittingAssignmentHasInvalidType()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.Parts.First().LightEmittingSurfaces[0].FaceAssignments[0] = new InvalidFaceAssignmentDto();

            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            action.Should().Throw<Exception>().WithMessage(@"Invalid AssignmentDto in GeometryNodeDto: *");
        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenLightEmittingFaceAssignemntIsNull()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.Parts.First().LightEmittingSurfaces[0].FaceAssignments[0] = null;

            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            action.Should().Throw<Exception>().WithMessage(@"Invalid AssignmentDto in GeometryNodeDto: *");
        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenGeometryNodoHasNoGeometrySource()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.Parts.First().GeometrySource = null;

            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            action.Should().Throw<Exception>().WithMessage(@"GeometryNodeDto must have a GeometrySource!");
        }

        private class InvalidGeometrySourceDto : GeometrySourceDto
        {

        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenGeometryNodoHasInvalidGeometrySource()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.Parts.First().GeometrySource = new InvalidGeometrySourceDto();

            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            action.Should().Throw<Exception>().WithMessage(@"Unknown GeometrySource type in GeometryNodeDto: *");
        }

        private class InvalidGeometryDefinitionDto : GeometryDefinitionDto
        {

        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenGeometryDefinitionIsNotAvailable()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.GeometryDefinitions.First().Id = "xyz";

            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            action.Should().Throw<Exception>().WithMessage(@"LuminaireDto has no geometry definition with the id cube");
        }

        [Test]
        public void BuildLuminaireFromDto_ShouldThrowArgumentException_WhenGeometryDefinitionIsNotValid()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.GeometryDefinitions[0] = new InvalidGeometryDefinitionDto {Id = "cube"};

            Action action = () => new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            action.Should().Throw<Exception>().WithMessage(@"Unknown GeometryDefinitionDto type in LuminaireDto: *");

        }

        [Test]
        public void BuildLuminaireFromDto_ShouldNotThrowAndConvertNullVectorToZeroVector()
        {
            CreateExample000TestData(out var luminaireDto, out var exampleDirectory);
            luminaireDto.Parts.First().Position = null;

            var resultLuminaire = new LuminaireFromDtoConstructor().BuildLuminaireFromDto(Builder.NewLuminaire(), luminaireDto, exampleDirectory);
            
            resultLuminaire.Parts.First().Position.Should().BeEquivalentTo(new Vector3(0, 0, 0));
        }

        [Test]
        [TestCaseSource(nameof(ExampleDirectories))]
        public void BuildLuminaireFromDto_ShouldCreateExpectedLuminaire(string exampleDirectory)
        {
            var exampleName = Path.GetFileName(exampleDirectory).ToLower();

            if (!Setup.ExampleBuilderMapping.TryGetValue(exampleName, out var builderFunction))
                throw new Exception($"No test code for example '{exampleName}' available!");

            var sourceBuilder = Builder.NewLuminaire();
            builderFunction(sourceBuilder);
            var expectedLuminaireData = sourceBuilder.Luminaire;

            var testBuilder = Builder.NewLuminaire();
            var exampleLuminaireDto = new XmlDtoSerializer().Deserialize(Path.Combine(exampleDirectory, Constants.L3dXmlFilename));

            var resultLuminaireData = new LuminaireFromDtoConstructor().BuildLuminaireFromDto(testBuilder, exampleLuminaireDto, exampleDirectory);

            resultLuminaireData.Should().BeEquivalentTo(expectedLuminaireData, ExampleDtoOptions);
        }
    }
}
