using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using L3D.Net.Data;
using L3D.Net.Mapper.V0_11_0;
using L3D.Net.XML.V0_11_0.Dto;
using NUnit.Framework;

namespace L3D.Net.Tests.Mapper.V0_11_0
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class LuminaireMapperTests : MapperTestBase
    {
        private static IEnumerable<TestCaseData> TestCases()
        {
            var utcNow = DateTime.UtcNow;
            yield return new TestCaseData(
                    new LuminaireDto { Header = new HeaderDto { FormatVersion = new FormatVersionDto() } },
                    new Luminaire { Header = new Header { FormatVersion = new FormatVersion() } })
                .SetArgDisplayNames("<new()>", "<new()>");
            yield return new TestCaseData(
                    new LuminaireDto { Header = new HeaderDto { Name = "name1", Description = "desc", CreatedWithApplication = "app", CreationTimeCode = utcNow, FormatVersion = new FormatVersionDto { Major = 1, Minor = 2, PreRelease = 3, PreReleaseSpecified = true } } },
                    new Luminaire { Header = new Header { Name = "name1", Description = "desc", CreatedWithApplication = "app", CreationTimeCode = utcNow, FormatVersion = new FormatVersion { Major = 1, Minor = 2, PreRelease = 3, PreReleaseSpecified = true } } })
                .SetArgDisplayNames(nameof(LuminaireDto.Header), nameof(Luminaire.Header));
            yield return new TestCaseData(
                    new LuminaireDto { GeometryDefinitions = new List<GeometryDefinitionDto> { new GeometryFileDefinitionDto { GeometryId = "id" } }, Header = new HeaderDto { FormatVersion = new FormatVersionDto() } },
                    new Luminaire { GeometryDefinitions = new List<GeometryFileDefinition> { new() { GeometryId = "id" } }, Header = new Header { FormatVersion = new FormatVersion() } })
                .SetArgDisplayNames(nameof(LuminaireDto.GeometryDefinitions), nameof(Luminaire.GeometryDefinitions));
            yield return new TestCaseData(
                    new LuminaireDto
                    {
                        Header = new HeaderDto { FormatVersion = new FormatVersionDto() },
                        Parts = new List<GeometryPartDto>
                        {
                            new()
                            {
                                Name = "name2",
                                Position = new Vector3Dto { X = 0.1f, Y = 0.2f, Z = 0.3f },
                                Rotation = new Vector3Dto { X = 0.4f, Y = 0.5f, Z = 0.6f },
                                IncludedInMeasurement = true,
                                ElectricalConnectors = new Vector3Dto[] { new(){ X =  0.7f, Y = 0.8f, Z = 0.9f } },
                                GeometryReference = new GeometryReferenceDto { GeometryId = "id" },
                                LightEmittingObjects = new LightEmittingPartDto[]
                                {
                                    new(new RectangleDto
                                    {
                                        SizeX = 50,
                                        SizeY = 60
                                    })
                                    {
                                        Name = "name3",
                                        Rotation = new Vector3Dto { X = 1.0f, Y = 1.1f, Z = 1.2f },
                                        Position = new Vector3Dto { X = 1.3f, Y = 1.4f, Z = 1.5f },
                                        LuminousHeights = new LuminousHeightsDto
                                        {
                                            C0 = 10,
                                            C90 = 20,
                                            C180 = 30,
                                            C270 = 40
                                        }
                                    }
                                },
                                LightEmittingSurfaces = new LightEmittingSurfacePartDto[]
                                {
                                    new()
                                    {
                                        Name = "name3",
                                        FaceAssignments = new List<FaceAssignmentDto>
                                        {
                                            new FaceRangeAssignmentDto
                                            {
                                                FaceIndexBegin = 1,
                                                FaceIndexEnd = 2,
                                                GroupIndex = 3
                                            }
                                        },
                                        LightEmittingPartIntensityMapping = new List<LightEmittingObjectReferenceDto>
                                        {
                                            new()
                                            {
                                                Intensity = 0.8,
                                                LightEmittingPartName = "name4"
                                            }
                                        }
                                    }
                                },
                                Joints = new JointPartDto[]
                                {
                                    new()
                                    {
                                        Name = "name5",
                                        Rotation = new Vector3Dto { X = 1.6f, Y = 1.7f, Z = 1.8f },
                                        Position = new Vector3Dto { X = 1.9f, Y = 2.0f, Z = 2.1f },
                                        DefaultRotation = new Vector3Dto { X = 2.2f, Y = 2.3f, Z = 2.4f },
                                        XAxis = new AxisRotationDto { Min = 2.5, Max = 2.6, Step = 2.7 },
                                        YAxis = new AxisRotationDto { Min = 2.8, Max = 2.9, Step = 3.0 },
                                        ZAxis = new AxisRotationDto { Min = 3.1, Max = 3.2, Step = 3.3 }
                                    }
                                },
                                PendulumConnectors = new Vector3Dto[]
                                {
                                    new()
                                    {
                                        X = 3.4f,
                                        Y = 3.5f,
                                        Z = 3.6f
                                    }
                                },
                                Sensors = new SensorPartDto[]
                                {
                                    new()
                                    {
                                        Name = "name6",
                                        Position = new Vector3Dto
                                        {
                                            X = 3.4f,
                                            Y = 3.5f,
                                            Z = 3.6f
                                        },
                                        Rotation = new Vector3Dto
                                        {
                                            X = 3.7f,
                                            Y = 3.8f,
                                            Z = 3.9f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Luminaire
                    {
                        Header = new Header { FormatVersion = new FormatVersion() },
                        Parts = new List<GeometryPart>
                        {
                            new()
                            {
                                Name = "name2",
                                Position = new Vector3 { X = 0.1f, Y = 0.2f, Z = 0.3f },
                                Rotation = new Vector3 { X = 0.4f, Y = 0.5f, Z = 0.6f },
                                IncludedInMeasurement = true,
                                ElectricalConnectors = new List<Vector3> { new(){ X =  0.7f, Y = 0.8f, Z = 0.9f } },
                                GeometryReference = new GeometryFileDefinition { GeometryId = "id" },
                                LightEmittingObjects = new List<LightEmittingPart>
                                {
                                    new(new Rectangle
                                    {
                                        SizeX = 50,
                                        SizeY = 60
                                    })
                                    {
                                        Name = "name3",
                                        Rotation = new Vector3 { X = 1.0f, Y = 1.1f, Z = 1.2f },
                                        Position = new Vector3 { X = 1.3f, Y = 1.4f, Z = 1.5f },
                                        LuminousHeights = new LuminousHeights
                                        {
                                            C0 = 10,
                                            C90 = 20,
                                            C180 = 30,
                                            C270 = 40
                                        }
                                    }
                                },
                                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                                {
                                    new()
                                    {
                                        Name = "name3",
                                        FaceAssignments = new List<FaceAssignment>
                                        {
                                            new FaceRangeAssignment
                                            {
                                                FaceIndexBegin = 1,
                                                FaceIndexEnd = 2,
                                                GroupIndex = 3
                                            }
                                        },
                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                        {
                                            ["name4"] = 0.8
                                        }
                                    }
                                },
                                Joints = new List<JointPart>
                                {
                                    new()
                                    {
                                        Name = "name5",
                                        Rotation = new Vector3 { X = 1.6f, Y = 1.7f, Z = 1.8f },
                                        Position = new Vector3 { X = 1.9f, Y = 2.0f, Z = 2.1f },
                                        DefaultRotation = new Vector3 { X = 2.2f, Y = 2.3f, Z = 2.4f },
                                        XAxis = new AxisRotation { Min = 2.5, Max = 2.6, Step = 2.7 },
                                        YAxis = new AxisRotation { Min = 2.8, Max = 2.9, Step = 3.0 },
                                        ZAxis = new AxisRotation { Min = 3.1, Max = 3.2, Step = 3.3 }
                                    }
                                },
                                PendulumConnectors = new List<Vector3>
                                {
                                    new()
                                    {
                                        X = 3.4f,
                                        Y = 3.5f,
                                        Z = 3.6f
                                    }
                                },
                                Sensors = new List<SensorPart>
                                {
                                    new()
                                    {
                                        Name = "name6",
                                        Position = new Vector3
                                        {
                                            X = 3.4f,
                                            Y = 3.5f,
                                            Z = 3.6f
                                        },
                                        Rotation = new Vector3
                                        {
                                            X = 3.7f,
                                            Y = 3.8f,
                                            Z = 3.9f
                                        }
                                    }
                                }
                            }
                        }
                    })
                .SetArgDisplayNames(nameof(LuminaireDto.Parts), nameof(Luminaire.Parts));
            yield return new TestCaseData(
                    new LuminaireDto
                    {
                        Parts = new List<GeometryPartDto>
                        {
                            new()
                            {
                                Name = "name2",
                                Position = new Vector3Dto { X = 0.1f, Y = 0.2f, Z = 0.3f },
                                Rotation = new Vector3Dto { X = 0.4f, Y = 0.5f, Z = 0.6f },
                                IncludedInMeasurement = true,
                                ElectricalConnectors = new Vector3Dto[] { new(){ X =  0.7f, Y = 0.8f, Z = 0.9f } },
                                GeometryReference = new GeometryReferenceDto { GeometryId = "id" },
                                LightEmittingObjects = new LightEmittingPartDto[]
                                {
                                    new(new RectangleDto
                                    {
                                        SizeX = 50,
                                        SizeY = 60
                                    })
                                    {
                                        Name = "name3",
                                        Rotation = new Vector3Dto { X = 1.0f, Y = 1.1f, Z = 1.2f },
                                        Position = new Vector3Dto { X = 1.3f, Y = 1.4f, Z = 1.5f },
                                        LuminousHeights = new LuminousHeightsDto
                                        {
                                            C0 = 10,
                                            C90 = 20,
                                            C180 = 30,
                                            C270 = 40
                                        }
                                    }
                                },
                                LightEmittingSurfaces = new LightEmittingSurfacePartDto[]
                                {
                                    new()
                                    {
                                        Name = "name3",
                                        FaceAssignments = new List<FaceAssignmentDto>
                                        {
                                            new FaceRangeAssignmentDto
                                            {
                                                FaceIndexBegin = 1,
                                                FaceIndexEnd = 2,
                                                GroupIndex = 3
                                            }
                                        },
                                        LightEmittingPartIntensityMapping = new List<LightEmittingObjectReferenceDto>
                                        {
                                            new()
                                            {
                                                Intensity = 0.8,
                                                LightEmittingPartName = "name4"
                                            }
                                        }
                                    }
                                },
                                Joints = new JointPartDto[]
                                {
                                    new()
                                    {
                                        Name = "name5",
                                        Rotation = new Vector3Dto { X = 1.6f, Y = 1.7f, Z = 1.8f },
                                        Position = new Vector3Dto { X = 1.9f, Y = 2.0f, Z = 2.1f },
                                        DefaultRotation = new Vector3Dto { X = 2.2f, Y = 2.3f, Z = 2.4f },
                                        XAxis = new AxisRotationDto { Min = 2.5, Max = 2.6, Step = 2.7 },
                                        YAxis = new AxisRotationDto { Min = 2.8, Max = 2.9, Step = 3.0 },
                                        ZAxis = new AxisRotationDto { Min = 3.1, Max = 3.2, Step = 3.3 }
                                    }
                                },
                                PendulumConnectors = new Vector3Dto[]
                                {
                                    new()
                                    {
                                        X = 3.4f,
                                        Y = 3.5f,
                                        Z = 3.6f
                                    }
                                },
                                Sensors = new SensorPartDto[]
                                {
                                    new()
                                    {
                                        Name = "name6",
                                        Position = new Vector3Dto
                                        {
                                            X = 3.4f,
                                            Y = 3.5f,
                                            Z = 3.6f
                                        },
                                        Rotation = new Vector3Dto
                                        {
                                            X = 3.7f,
                                            Y = 3.8f,
                                            Z = 3.9f
                                        }
                                    }
                                }
                            }
                        },
                        Header = new HeaderDto { Name = "name1", Description = "desc", CreatedWithApplication = "app", CreationTimeCode = utcNow, FormatVersion = new FormatVersionDto() },
                        GeometryDefinitions = new List<GeometryDefinitionDto> { new GeometryFileDefinitionDto { GeometryId = "id" } }
                    },
                    new Luminaire
                    {
                        Parts = new List<GeometryPart>
                        {
                            new()
                            {
                                Name = "name2",
                                Position = new Vector3 { X = 0.1f, Y = 0.2f, Z = 0.3f },
                                Rotation = new Vector3 { X = 0.4f, Y = 0.5f, Z = 0.6f },
                                IncludedInMeasurement = true,
                                ElectricalConnectors = new List<Vector3> { new(){ X =  0.7f, Y = 0.8f, Z = 0.9f } },
                                GeometryReference = new GeometryFileDefinition { GeometryId = "id" },
                                LightEmittingObjects = new List<LightEmittingPart>
                                {
                                    new(new Rectangle
                                    {
                                        SizeX = 50,
                                        SizeY = 60
                                    })
                                    {
                                        Name = "name3",
                                        Rotation = new Vector3 { X = 1.0f, Y = 1.1f, Z = 1.2f },
                                        Position = new Vector3 { X = 1.3f, Y = 1.4f, Z = 1.5f },
                                        LuminousHeights = new LuminousHeights
                                        {
                                            C0 = 10,
                                            C90 = 20,
                                            C180 = 30,
                                            C270 = 40
                                        }
                                    }
                                },
                                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                                {
                                    new()
                                    {
                                        Name = "name3",
                                        FaceAssignments = new List<FaceAssignment>
                                        {
                                            new FaceRangeAssignment
                                            {
                                                FaceIndexBegin = 1,
                                                FaceIndexEnd = 2,
                                                GroupIndex = 3
                                            }
                                        },
                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                        {
                                            ["name4"] = 0.8
                                        }
                                    }
                                },
                                Joints = new List<JointPart>
                                {
                                    new()
                                    {
                                        Name = "name5",
                                        Rotation = new Vector3 { X = 1.6f, Y = 1.7f, Z = 1.8f },
                                        Position = new Vector3 { X = 1.9f, Y = 2.0f, Z = 2.1f },
                                        DefaultRotation = new Vector3 { X = 2.2f, Y = 2.3f, Z = 2.4f },
                                        XAxis = new AxisRotation { Min = 2.5, Max = 2.6, Step = 2.7 },
                                        YAxis = new AxisRotation { Min = 2.8, Max = 2.9, Step = 3.0 },
                                        ZAxis = new AxisRotation { Min = 3.1, Max = 3.2, Step = 3.3 }
                                    }
                                },
                                PendulumConnectors = new List<Vector3>
                                {
                                    new()
                                    {
                                        X = 3.4f,
                                        Y = 3.5f,
                                        Z = 3.6f
                                    }
                                },
                                Sensors = new List<SensorPart>
                                {
                                    new()
                                    {
                                        Name = "name6",
                                        Position = new Vector3
                                        {
                                            X = 3.4f,
                                            Y = 3.5f,
                                            Z = 3.6f
                                        },
                                        Rotation = new Vector3
                                        {
                                            X = 3.7f,
                                            Y = 3.8f,
                                            Z = 3.9f
                                        }
                                    }
                                }
                            }
                        },
                        Header = new Header { Name = "name1", Description = "desc", CreatedWithApplication = "app", CreationTimeCode = utcNow, FormatVersion = new FormatVersion() },
                        GeometryDefinitions = new List<GeometryFileDefinition> { new() { GeometryId = "id" } }
                    })
                .SetArgDisplayNames("<filled>", "<filled>");
        }

        private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDataModel(LuminaireDto element, Luminaire expected)
        {
            LuminaireMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected, opt => opt
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(50)))
                .WhenTypeIs<DateTime>());
        }

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDto(LuminaireDto expected, Luminaire element)
        {
            LuminaireMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected, opt => opt
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(50)))
                .WhenTypeIs<DateTime>());
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDataModel(LuminaireDto element, Luminaire expected)
        {
            LuminaireMapper.Instance.Convert(element).Should().BeEquivalentTo(expected, opt => opt
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(50)))
                .WhenTypeIs<DateTime>());
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDto(LuminaireDto expected, Luminaire element)
        {
            LuminaireMapper.Instance.Convert(element).Should().BeEquivalentTo(expected, opt => opt
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(50)))
                .WhenTypeIs<DateTime>());
        }
    }
}
