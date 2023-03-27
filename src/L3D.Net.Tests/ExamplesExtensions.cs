using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace L3D.Net.Tests;

public static class ExamplesExtensions
{
    private static readonly IObjParser ObjParser = Geometry.ObjParser.Instance;

    public static Luminaire BuildExample000(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_000");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "cube.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var geometryDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(cubeObjPath, NullLogger.Instance),
            FileName = cubeObjPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            geometryDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "luminaire",
                LightEmittingObjects = new List<LightEmittingPart>
                {
                    new()
                    {
                        Name = "leo",
                        Shape = new Rectangle
                        {
                            SizeX = 0.5,
                            SizeY = 0.25
                        }
                    }
                },
                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                {
                    new()
                    {
                        Name = "les",
                        FaceAssignments = new List<FaceAssignment>
                        {
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        },
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                },
                GeometrySource = geometryDefinition
            }
        };

        return luminaire;
    }

    public static Luminaire BuildExample001(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_001");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "cube.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var geometryDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(cubeObjPath, NullLogger.Instance),
            FileName = cubeObjPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            geometryDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "luminaire",
                LightEmittingObjects = new List<LightEmittingPart>
                {
                    new()
                    {
                        Name = "leo",
                        Shape = new Rectangle
                        {
                            SizeX = 0.5,
                            SizeY = 0.25
                        },
                        Position = new Vector3
                        {
                            X = -0.25f,
                            Y = -0.125f,
                            Z = 0.05f
                        }
                    }
                },
                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                {
                    new()
                    {
                        Name = "les",
                        FaceAssignments = new List<FaceAssignment>
                        {
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        },
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                },
                Position = new Vector3
                {
                    X = -0.25f,
                    Y = -0.125f,
                    Z = 0.05f
                },
                GeometrySource = geometryDefinition
            }
        };

        return luminaire;
    }

    public static Luminaire BuildExample002(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_002");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var baseArmConObjPath = Path.Combine(exampleDirectory, "base-arm-con", "base-arm-con.obj");
        var armObjPath = Path.Combine(exampleDirectory, "arm", "arm.obj");
        var armHeadConObjPath = Path.Combine(exampleDirectory, "arm-head-con", "arm-head-con.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0",
            Name = "First example",
            Description = "First ever xml luminaire geometry description"
        };

        var baseDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseObjPath, NullLogger.Instance),
            FileName = baseObjPath
        };
        var baseArmDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseArmConObjPath, NullLogger.Instance),
            FileName = baseArmConObjPath
        };
        var armDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(armObjPath, NullLogger.Instance),
            FileName = armObjPath
        };
        var armHeadDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(armHeadConObjPath, NullLogger.Instance),
            FileName = armHeadConObjPath
        };
        var headDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(headObjPath, NullLogger.Instance),
            FileName = headObjPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            baseDefinition,
            baseArmDefinition,
            armDefinition,
            armHeadDefinition,
            headDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "base",
                GeometrySource = baseDefinition,
                Joints = new List<JointPart>
                {
                    new()
                    {
                        Name = "base-con-hinge",
                        Position = new Vector3
                        {
                            X = 0f,
                            Y = 0.05f,
                            Z = 0.05f
                        },
                        ZAxis = new()
                        {
                            Min = -15,
                            Max = 15,
                            Step = 1
                        },
                        Geometries = new List<GeometryPart>
                        {
                            new()
                            {
                                Name = "base-arm-con",
                                Position = new Vector3
                                {
                                    X = 0f,
                                    Y = -0.05f,
                                    Z = -0.05f
                                },
                                GeometrySource = baseArmDefinition,
                                Joints = new List<JointPart>
                                {
                                    new()
                                    {
                                        Name = "con-arm-hinge",
                                        Position = new Vector3
                                        {
                                            X = 0f,
                                            Y = 0.05f,
                                            Z = 0.085f
                                        },
                                        XAxis = new()
                                        {
                                            Min = 0,
                                            Max = 60,
                                            Step = 1
                                        },
                                        DefaultRotation = new Vector3
                                        {
                                            X = 30f,
                                            Y = 0f,
                                            Z = 0f
                                        },
                                        Geometries = new List<GeometryPart>
                                        {
                                            new()
                                            {
                                                Name = "arm",
                                                GeometrySource = armDefinition,
                                                Position = new Vector3
                                                {
                                                    X = 0f,
                                                    Y = -0.05f,
                                                    Z = -0.085f
                                                },
                                                Joints = new List<JointPart>
                                                {
                                                    new()
                                                    {
                                                        Name = "arm-con-hinge",
                                                        Position = new Vector3
                                                        {
                                                            X = 0f,
                                                            Y = 0.05f,
                                                            Z = 0.485f
                                                        },
                                                        Rotation = new Vector3
                                                        {
                                                            X = 90f,
                                                            Y = 0f,
                                                            Z = 0f
                                                        },
                                                        XAxis = new()
                                                        {
                                                            Min = -60,
                                                            Max = 0,
                                                            Step = 1
                                                        },
                                                        DefaultRotation = new Vector3
                                                        {
                                                            X = -30f,
                                                            Y = 0f,
                                                            Z = 0f
                                                        },
                                                        Geometries = new List<GeometryPart>
                                                        {
                                                            new()
                                                            {
                                                                Name = "arm-head-con",
                                                                Position = new Vector3
                                                                {
                                                                    X = 0f,
                                                                    Y = -0.05f,
                                                                    Z = -0.485f
                                                                },
                                                                GeometrySource = armHeadDefinition,
                                                                Joints = new List<JointPart>
                                                                {
                                                                    new()
                                                                    {
                                                                        Name = "con-head-hinge",
                                                                        Position = new Vector3
                                                                        {
                                                                            X = 0f,
                                                                            Y = 0.05f,
                                                                            Z = 0.525f
                                                                        },
                                                                        ZAxis = new AxisRotation
                                                                        {
                                                                            Min = -15,
                                                                            Max = 15,
                                                                            Step = 1
                                                                        },
                                                                        Geometries = new List<GeometryPart>
                                                                        {
                                                                            new()
                                                                            {
                                                                                Name = "head",
                                                                                GeometrySource = headDefinition,
                                                                                Position = new Vector3
                                                                                {
                                                                                    X = 0f,
                                                                                    Y = -0.05f,
                                                                                    Z = -0.525f
                                                                                },
                                                                                IncludedInMeasurement = false,
                                                                                LightEmittingObjects = new List<LightEmittingPart>
                                                                                {
                                                                                    new()
                                                                                    {
                                                                                        Name = "leo",
                                                                                        Shape = new Rectangle
                                                                                        {
                                                                                            SizeX = 0.08,
                                                                                            SizeY = 0.18
                                                                                        },
                                                                                        Position = new Vector3
                                                                                        {
                                                                                            X = 0f,
                                                                                            Y = 0.04631f,
                                                                                            Z = 0.6771f
                                                                                        },
                                                                                        Rotation = new Vector3
                                                                                        {
                                                                                            X = 90f,
                                                                                            Y = 0f,
                                                                                            Z = 180f
                                                                                        }
                                                                                    }
                                                                                },
                                                                                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                                                                                {
                                                                                    new()
                                                                                    {
                                                                                        Name = "les",
                                                                                        FaceAssignments = new List<FaceAssignment>
                                                                                        {
                                                                                            new FaceRangeAssignment
                                                                                            {
                                                                                                FaceIndexBegin = 16,
                                                                                                FaceIndexEnd = 21
                                                                                            }
                                                                                        },
                                                                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                                                                        {
                                                                                            ["leo"] = 1
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        return luminaire;
    }

    public static Luminaire BuildExample003(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_003");
        var objPath = Path.Combine(exampleDirectory, "luminaire", "luminaire.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0"
        };

        var bodyDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(objPath, NullLogger.Instance),
            FileName = objPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            bodyDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "luminaire",
                GeometrySource = bodyDefinition,
                LightEmittingObjects = new List<LightEmittingPart>
                {
                    new()
                    {
                        Name = "leo",
                        Shape = new Circle
                        {
                            Diameter = 0.2
                        }
                    }
                },
                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                {
                    new()
                    {
                        Name = "les",
                        FaceAssignments = new List<FaceAssignment>
                        {
                            new FaceRangeAssignment
                            {
                                FaceIndexBegin = 1199,
                                FaceIndexEnd = 1235
                            }
                        },
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                }
            }
        };

        return luminaire;
    }

    public static Luminaire BuildExample004(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_004");
        var objPath = Path.Combine(exampleDirectory, "luminaire", "luminaire.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0"
        };

        var bodyDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(objPath, NullLogger.Instance),
            FileName = objPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            bodyDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "luminaire",
                GeometrySource = bodyDefinition,
                Position = new Vector3
                {
                    X = 0f,
                    Y = 0f,
                    Z = -0.5f
                },
                LightEmittingObjects = new List<LightEmittingPart>
                {
                    new()
                    {
                        Name = "leo_top",
                        Shape = new Rectangle
                        {
                            SizeX = 0.15,
                            SizeY = 1.0
                        },
                        Position = new Vector3
                        {
                            X = 0f,
                            Y = 0f,
                            Z = 0.0375f
                        },
                        Rotation = new Vector3
                        {
                            X = 180f,
                            Y = 0f,
                            Z = 90f
                        }
                    },
                    new()
                    {
                        Name = "leo_bottom",
                        Shape = new Rectangle
                        {
                            SizeX = 0.17,
                            SizeY = 1.175
                        },
                        Position = new Vector3
                        {
                            X = 0f,
                            Y = 0f,
                            Z = 0.0025f
                        },
                        Rotation = new Vector3
                        {
                            X = 0f,
                            Y = 0f,
                            Z = -90f
                        }
                    }
                },
                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                {
                    new()
                    {
                        Name = "les_top",
                        FaceAssignments = new List<FaceAssignment>
                        {
                            new FaceRangeAssignment
                            {
                                FaceIndexBegin = 84,
                                FaceIndexEnd = 85
                            }
                        },
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo_top"] = 1
                        }
                    },
                    new()
                    {
                        Name = "les_bottom",
                        FaceAssignments = new List<FaceAssignment>
                        {
                            new FaceRangeAssignment
                            {
                                FaceIndexBegin = 90,
                                FaceIndexEnd = 91
                            }
                        },
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo_bottom"] = 1
                        }
                    }
                },
                ElectricalConnectors = new List<Vector3>
                {
                    new()
                    {
                        X = -0.575f,
                        Y = 0f,
                        Z = 0.04f
                    }
                },
                PendulumConnectors = new List<Vector3>
                {
                    new()
                    {
                        X = -0.55f,
                        Y = 0f,
                        Z = 0.04f
                    },
                    new()
                    {
                        X = 0.55f,
                        Y = 0f,
                        Z = 0.04f
                    }
                }
            }
        };

        return luminaire;
    }

    public static Luminaire BuildExample005(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_005");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0",
            Name = "Another example",
            Description = "Example luminaire 4"
        };

        var baseDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseObjPath, NullLogger.Instance),
            FileName = baseObjPath
        };
        var headDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(headObjPath, NullLogger.Instance),
            FileName = headObjPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            baseDefinition,
            headDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "base",
                GeometrySource = baseDefinition,
                Joints = new List<JointPart>
                {
                    new()
                    {
                        Name = "head-hinge",
                        Position = new Vector3
                        {
                            X = 0f,
                            Y = 0f,
                            Z = 0.5f
                        },
                        Rotation = new Vector3
                        {
                            X = 45f,
                            Y = 0f,
                            Z = 0f
                        },
                        ZAxis = new AxisRotation
                        {
                            Min = -30,
                            Max = 30,
                            Step = 1
                        },
                        Geometries = new List<GeometryPart>
                        {
                            new()
                            {
                                Name = "head",
                                GeometrySource = headDefinition,
                                Position = new Vector3
                                {
                                    X = 0f,
                                    Y = -0.3535f,
                                    Z = -0.3535f
                                },
                                Rotation = new Vector3
                                {
                                    X = -45f,
                                    Y = 0f,
                                    Z = 0f
                                },
                                IncludedInMeasurement = false,
                                LightEmittingObjects = new List<LightEmittingPart>
                                {
                                    new()
                                    {
                                        Name = "leo",
                                        Shape = new Rectangle
                                        {
                                            SizeX = 0.15,
                                            SizeY = 0.035
                                        },
                                        Position = new Vector3
                                        {
                                            X = 0f,
                                            Y = -0.064f,
                                            Z = 0.475f
                                        },
                                        Rotation = new Vector3
                                        {
                                            X = -45f,
                                            Y = 0f,
                                            Z = 0f
                                        }
                                    }
                                },
                                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                                {
                                    new()
                                    {
                                        Name = "les",
                                        FaceAssignments = new List<FaceAssignment>
                                        {
                                            new FaceRangeAssignment
                                            {
                                                FaceIndexBegin = 158,
                                                FaceIndexEnd = 179
                                            }
                                        },
                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                        {
                                            ["leo"] = 1
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        return luminaire;
    }

    public static Luminaire BuildExample006(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_006");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var baseHeadConeObjPath = Path.Combine(exampleDirectory, "base-head-con", "base-head-con.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Experimental"
        };

        var baseDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(baseObjPath, NullLogger.Instance),
            FileName = baseObjPath
        };
        var baseHeadConnectionDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(baseHeadConeObjPath, NullLogger.Instance),
            FileName = baseHeadConeObjPath
        };
        var headDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(headObjPath, NullLogger.Instance),
            FileName = headObjPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            baseDefinition,
            baseHeadConnectionDefinition,
            headDefinition
        };

        GeometryPart CreatePart(int index, float yPosition) => new()
        {
            Name = $"part-{index}",
            GeometrySource = baseHeadConnectionDefinition,
            Joints = new List<JointPart>
            {
                new()
                {
                    Name = $"head-joint-{index}",
                    Position = new Vector3
                    {
                        X = 0f,
                        Y = yPosition,
                        Z = -0.128f
                    },
                    YAxis = new AxisRotation
                    {
                        Min = -30,
                        Max = 65,
                        Step = 1
                    },
                    Geometries = new List<GeometryPart>
                    {
                        new()
                        {
                            Name = $"head-{index}",
                            GeometrySource = headDefinition,
                            LightEmittingObjects = new List<LightEmittingPart>
                            {
                                new()
                                {
                                    Name = $"leo-{index}",
                                    Shape = new Circle
                                    {
                                        Diameter = 0.1
                                    },
                                    Position = new Vector3
                                    {
                                        X = 0.045f,
                                        Y = -0.043f,
                                        Z = 0f
                                    },
                                    Rotation = new Vector3
                                    {
                                        X = 0f,
                                        Y = -90f,
                                        Z = 0f
                                    }
                                }
                            },
                            LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                            {
                                new()
                                {
                                    Name = $"les-{index}",
                                    FaceAssignments = new List<FaceAssignment>
                                    {
                                        new FaceRangeAssignment
                                        {
                                            FaceIndexBegin = 50,
                                            FaceIndexEnd = 97
                                        }
                                    },
                                    LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                    {
                                        [$"leo-{index}"] = 1
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "base",
                GeometrySource = baseDefinition,
                Joints = new List<JointPart>
                {
                    new()
                    {
                        Name = "part-joint-0",
                        Position = new Vector3
                        {
                            X = 0f,
                            Y = -0.55f,
                            Z = -0.017f
                        },
                        ZAxis = new AxisRotation
                        {
                            Min = -180,
                            Max = 180,
                            Step = 1
                        },
                        Geometries = new List<GeometryPart>
                        {
                            CreatePart(0, -0.55f),
                            CreatePart(1, -0.15f),
                            CreatePart(2, 0.25f),
                            CreatePart(3, 0.65f),
                        }
                    }
                }
            }
        };

        return luminaire;
    }

    public static Luminaire BuildExample007(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_007");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var baseHeadConeObjPath = Path.Combine(exampleDirectory, "base-head-con", "base-head-con.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Experimental"
        };

        var baseDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseObjPath, NullLogger.Instance),
            FileName = baseObjPath
        };
        var baseHeadConnectionDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(baseHeadConeObjPath, NullLogger.Instance),
            FileName = baseHeadConeObjPath
        };
        var headDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(headObjPath, NullLogger.Instance),
            FileName = headObjPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            baseDefinition,
            baseHeadConnectionDefinition,
            headDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "base",
                GeometrySource = baseDefinition,
                Joints = new List<JointPart>
                {
                    new()
                    {
                        Name = "base-head-con-joint-0",
                        Position = new Vector3
                        {
                            X = -0.4f,
                            Y = 0f,
                            Z = 0.0375f
                        },
                        ZAxis = new()
                        {
                            Min = -180,
                            Max = 180,
                            Step = 1
                        },
                        DefaultRotation = new Vector3
                        {
                            X = 0f,
                            Y = 0f,
                            Z = 0f
                        },
                        Geometries = new List<GeometryPart>
                        {
                            new()
                            {
                                Name = "base-head-con-0",
                                GeometrySource = baseHeadConnectionDefinition,
                                Joints = new List<JointPart>
                                {
                                    new()
                                    {
                                        Name = "head-joint-0",
                                        Position = new Vector3
                                        {
                                            X = -0.4f,
                                            Y = 0f,
                                            Z = 0.0375f
                                        },
                                        XAxis = new()
                                        {
                                            Min = -45,
                                            Max = 45,
                                            Step = 1
                                        },
                                        DefaultRotation = new Vector3
                                        {
                                            X = 0f,
                                            Y = 0f,
                                            Z = 0f
                                        },
                                        Geometries = new List<GeometryPart>
                                        {
                                            new()
                                            {
                                                Name = "head-0",
                                                GeometrySource = headDefinition,
                                                Position = new Vector3
                                                {
                                                    X = 0.4f,
                                                    Y = 0f,
                                                    Z = -0.0375f
                                                },
                                                LightEmittingObjects = new List<LightEmittingPart>
                                                {
                                                    new()
                                                    {
                                                        Name = "LEO0",
                                                        Shape = new Circle
                                                        {
                                                            Diameter = 0.0575
                                                        },
                                                        Position = new Vector3
                                                        {
                                                            X = -0.4f,
                                                            Y = 0f,
                                                            Z = -0.0375f
                                                        },
                                                        LuminousHeights = new LuminousHeights
                                                        {
                                                            C0 = 10,
                                                            C90 = 10,
                                                            C180 = 10,
                                                            C270 = 10
                                                        }
                                                    }
                                                },
                                                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                                                {
                                                    new()
                                                    {
                                                        Name = "LES0",
                                                        FaceAssignments = new List<FaceAssignment>
                                                        {
                                                            new FaceRangeAssignment()
                                                            {
                                                                FaceIndexBegin = 574,
                                                                FaceIndexEnd = 607
                                                            }
                                                        },
                                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                                        {
                                                            ["LEO0"] = 1
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new()
                    {
                        Name = "base-head-con-joint-1",
                        Position = new Vector3
                        {
                            X = -0.4f,
                            Y = 0f,
                            Z = 0.0375f
                        },
                        ZAxis = new()
                        {
                            Min = -180,
                            Max = 180,
                            Step = 1
                        },
                        DefaultRotation = new Vector3
                        {
                            X = 0f,
                            Y = 0f,
                            Z = 0f
                        },
                        Geometries = new List<GeometryPart>
                        {
                            new()
                            {
                                Name = "base-head-con-1",
                                GeometrySource = baseHeadConnectionDefinition,
                                Joints = new List<JointPart>
                                {
                                    new()
                                    {
                                        Name = "head-joint-1",
                                        Position = new Vector3
                                        {
                                            X = -0.4f,
                                            Y = 0f,
                                            Z = 0.0375f
                                        },
                                        XAxis = new()
                                        {
                                            Min = -45,
                                            Max = 45,
                                            Step = 1
                                        },
                                        DefaultRotation = new Vector3
                                        {
                                            X = 0f,
                                            Y = 0f,
                                            Z = 0f
                                        },
                                        Geometries = new List<GeometryPart>
                                        {
                                            new()
                                            {
                                                Name = "head-1",
                                                GeometrySource = headDefinition,
                                                Position = new Vector3
                                                {
                                                    X = 0.4f,
                                                    Y = 0f,
                                                    Z = -0.0375f
                                                },
                                                LightEmittingObjects = new List<LightEmittingPart>
                                                {
                                                    new()
                                                    {
                                                        Name = "LEO1",
                                                        Shape = new Circle
                                                        {
                                                            Diameter = 0.0575
                                                        },
                                                        Position = new Vector3
                                                        {
                                                            X = -0.4f,
                                                            Y = 0f,
                                                            Z = -0.0375f
                                                        },
                                                        LuminousHeights = new LuminousHeights
                                                        {
                                                            C0 = 10,
                                                            C90 = 10,
                                                            C180 = 10,
                                                            C270 = 10
                                                        }
                                                    }
                                                },
                                                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                                                {
                                                    new()
                                                    {
                                                        Name = "LES1",
                                                        FaceAssignments = new List<FaceAssignment>
                                                        {
                                                            new FaceRangeAssignment()
                                                            {
                                                                FaceIndexBegin = 574,
                                                                FaceIndexEnd = 607
                                                            }
                                                        },
                                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                                        {
                                                            ["LEO1"] = 1
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Sensors = new List<SensorPart>
                {
                    new()
                    {
                        Name = "Sensor"
                    }
                }
            }
        };

        return luminaire;
    }

    public static Luminaire BuildExample008(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_008");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "textured_cube.obj");

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var baseDefinition = new GeometrySource
        {
            GeometryId = "PN." + Guid.NewGuid(),
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(cubeObjPath, NullLogger.Instance),
            FileName = cubeObjPath
        };

        luminaire.GeometryDefinitions = new List<GeometrySource>
        {
            baseDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "luminaire",
                GeometrySource = baseDefinition,
                LightEmittingObjects = new List<LightEmittingPart>
                {
                    new()
                    {
                        Name = "leo",
                        Shape = new Rectangle
                        {
                            SizeX = 0.5,
                            SizeY = 0.25
                        }
                    }
                },
                LightEmittingSurfaces = new List<LightEmittingSurfacePart>
                {
                    new()
                    {
                        Name = "les",
                        FaceAssignments = new List<FaceAssignment>
                        {
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        },
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                }
            }
        };

        return luminaire;
    }
}