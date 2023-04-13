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
    public class PartMapperTests : MapperTestBase
    {
        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                    new LightEmittingSurfacePartDto(),
                    new LightEmittingSurfacePart())
                .SetArgDisplayNames("<new LightEmittingSurfacePartDto()>", "<new LightEmittingSurfacePart()>");
            yield return new TestCaseData(
                    new GeometryPartDto(),
                    new GeometryPart())
                .SetArgDisplayNames("<new GeometryPartDto()>", "<new GeometryPart()>");
            yield return new TestCaseData(
                    new JointPartDto(),
                    new JointPart())
                .SetArgDisplayNames("<new JointPartDto()>", "<new JointPart()>");
            yield return new TestCaseData(
                    new LightEmittingPartDto(new CircleDto { Diameter = 0.1 }),
                    new LightEmittingPart(new Circle { Diameter = 0.1 }))
                .SetArgDisplayNames("<new LightEmittingPartDto()>", "<new LightEmittingPart()>");
            yield return new TestCaseData(
                    new SensorPartDto(),
                    new SensorPart())
                .SetArgDisplayNames("<new SensorPartDto()>", "<new SensorPart()>");
            yield return new TestCaseData(
                    new LightEmittingSurfacePartDto
                    {
                        Name = "name",
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
                                LightEmittingPartName = "name1"
                            }
                        }
                    },
                    new LightEmittingSurfacePart
                    {
                        Name = "name",
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
                            ["name"] = 0.8
                        }
                    })
                .SetArgDisplayNames("<filled LightEmittingSurfacePartDto>", "<filled LightEmittingSurfacePart>");
            yield return new TestCaseData(
                    new GeometryPartDto
                    {
                        Name = "name2",
                        Position = new Vector3Dto { X = 0.1f, Y = 0.2f, Z = 0.3f },
                        Rotation = new Vector3Dto { X = 0.4f, Y = 0.5f, Z = 0.6f },
                        IncludedInMeasurement = true,
                        ElectricalConnectors = new List<Vector3Dto> { new() { X = 0.7f, Y = 0.8f, Z = 0.9f } },
                        GeometryReference = new GeometryReferenceDto { GeometryId = "id" },
                        LightEmittingObjects = new List<LightEmittingPartDto>
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
                        LightEmittingSurfaces = new List<LightEmittingSurfacePartDto>
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
                        Joints = new List<JointPartDto>
                        {
                            new()
                            {
                                Name = "name5",
                                Rotation = new Vector3Dto { X = 1.6f, Y = 1.7f, Z = 1.8f },
                                Position = new Vector3Dto { X = 1.9f, Y = 2.0f, Z = 2.1f },
                                DefaultRotation = new Vector3Dto { X = 2.2f, Y = 2.3f, Z = 2.4f },
                                XAxis = new AxisRotationDto { Min = 2.5, Max = 2.6, Step = 2.7 },
                                YAxis = new AxisRotationDto { Min = 2.8, Max = 2.9, Step = 3.0 },
                                ZAxis = new AxisRotationDto { Min = 3.1, Max = 3.2, Step = 3.3 },
                                Geometries = new List<GeometryPartDto>
                                {
                                    new()
                                    {
                                        Name = "nameX"
                                    }
                                }
                            }
                        },
                        PendulumConnectors = new List<Vector3Dto>
                        {
                            new()
                            {
                                X = 3.4f,
                                Y = 3.5f,
                                Z = 3.6f
                            }
                        },
                        Sensors = new List<SensorPartDto>
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
                    },
                    new GeometryPart
                    {
                        Name = "name2",
                        Position = new Vector3 { X = 0.1f, Y = 0.2f, Z = 0.3f },
                        Rotation = new Vector3 { X = 0.4f, Y = 0.5f, Z = 0.6f },
                        IncludedInMeasurement = true,
                        ElectricalConnectors = new List<Vector3> { new() { X = 0.7f, Y = 0.8f, Z = 0.9f } },
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
                                ZAxis = new AxisRotation { Min = 3.1, Max = 3.2, Step = 3.3 },
                                Geometries = new List<GeometryPart>
                                {
                                    new()
                                    {
                                        Name = "nameX"
                                    }
                                }
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
                    })
                .SetArgDisplayNames("<filled GeometryPartDto>", "<filled GeometryPart>");
            yield return new TestCaseData(
                    new JointPartDto
                    {
                        Name = "name5",
                        Rotation = new Vector3Dto { X = 1.6f, Y = 1.7f, Z = 1.8f },
                        Position = new Vector3Dto { X = 1.9f, Y = 2.0f, Z = 2.1f },
                        DefaultRotation = new Vector3Dto { X = 2.2f, Y = 2.3f, Z = 2.4f },
                        XAxis = new AxisRotationDto { Min = 2.5, Max = 2.6, Step = 2.7 },
                        YAxis = new AxisRotationDto { Min = 2.8, Max = 2.9, Step = 3.0 },
                        ZAxis = new AxisRotationDto { Min = 3.1, Max = 3.2, Step = 3.3 },
                        Geometries = new List<GeometryPartDto>
                        {
                            new()
                            {
                                Name = "nameX"
                            }
                        }
                    },
                    new JointPart
                    {
                        Name = "name5",
                        Rotation = new Vector3 { X = 1.6f, Y = 1.7f, Z = 1.8f },
                        Position = new Vector3 { X = 1.9f, Y = 2.0f, Z = 2.1f },
                        DefaultRotation = new Vector3 { X = 2.2f, Y = 2.3f, Z = 2.4f },
                        XAxis = new AxisRotation { Min = 2.5, Max = 2.6, Step = 2.7 },
                        YAxis = new AxisRotation { Min = 2.8, Max = 2.9, Step = 3.0 },
                        ZAxis = new AxisRotation { Min = 3.1, Max = 3.2, Step = 3.3 },
                        Geometries = new List<GeometryPart>
                        {
                            new()
                            {
                                Name = "nameX"
                            }
                        }
                    })
                .SetArgDisplayNames("<filled JointPartDto>", "<filled JointPart>");
            yield return new TestCaseData(
                    new LightEmittingPartDto(new RectangleDto
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
                    },
                    new LightEmittingPart(new Rectangle
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
                    })
                .SetArgDisplayNames("<filled LightEmittingPartDto>", "<filled LightEmittingPart>");
            yield return new TestCaseData(
                    new SensorPartDto
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
                    },
                    new SensorPart
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
                    })
                .SetArgDisplayNames("<filled SensorPartDto>", "<filled SensorPart>");
        }

        private static IEnumerable<TestCaseData> AllTestCases => NullableTestCases().Concat(TestCases());

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDataModel(PartDto element, Part expected)
        {
            PartMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(AllTestCases))]
        public void ConvertNullable_ShouldReturnCorrectDto(PartDto expected, Part element)
        {
            PartMapper.Instance.ConvertNullable(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDataModel(PartDto element, Part expected)
        {
            PartMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void Convert_ShouldReturnCorrectDto(PartDto expected, Part element)
        {
            PartMapper.Instance.Convert(element).Should().BeEquivalentTo(expected);
        }
    }
}
