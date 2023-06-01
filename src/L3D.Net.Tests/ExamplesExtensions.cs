using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using L3D.Net.XML.V0_11_0;

namespace L3D.Net.Tests;

public static class ExamplesExtensions
{
    private static readonly IObjParser ObjParser = Geometry.ObjParser.Instance;
    private static readonly ILuminaireResolver Resolver = LuminaireResolver.Instance;

    public static Luminaire BuildExample000(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_000");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "cube.obj");
        var fileName = Path.GetFileName(cubeObjPath);
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var geometryDefinition = new GeometryFileDefinition
        {
            GeometryId = "cube",
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(fileName, cache.Geometries["cube"], NullLogger.Instance),
            FileName = fileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
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
                    new(new Rectangle
                    {
                        SizeX = 0.5,
                        SizeY = 0.25
                    })
                    {
                        Name = "leo"
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
                GeometryReference = geometryDefinition
            }
        };

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample001(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_001");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "cube.obj");
        var cache = exampleDirectory.ToCache();
        var fileName = Path.GetFileName(cubeObjPath);

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var geometryDefinition = new GeometryFileDefinition
        {
            GeometryId = "cube",
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(fileName, cache.Geometries["cube"], NullLogger.Instance),
            FileName = fileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
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
                    new(new Rectangle
                    {
                        SizeX = 0.5,
                        SizeY = 0.25
                    })
                    {
                        Name = "leo",
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
                GeometryReference = geometryDefinition
            }
        };

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample002(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_002");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var baseArmConObjPath = Path.Combine(exampleDirectory, "base-arm-con", "base-arm-con.obj");
        var armObjPath = Path.Combine(exampleDirectory, "arm", "arm.obj");
        var armHeadConObjPath = Path.Combine(exampleDirectory, "arm-head-con", "arm-head-con.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");
        var cache = exampleDirectory.ToCache();
        var baseFileName = Path.GetFileName(baseObjPath);
        var baseArmFileName = Path.GetFileName(baseArmConObjPath);
        var armFileName = Path.GetFileName(armObjPath);
        var armHeadFileName = Path.GetFileName(armHeadConObjPath);
        var headFileName = Path.GetFileName(headObjPath);

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0",
            Name = "First example",
            Description = "First ever xml luminaire geometry description"
        };

        var baseDefinition = new GeometryFileDefinition
        {
            GeometryId = "base",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseFileName, cache.Geometries["base"], NullLogger.Instance),
            FileName = baseFileName
        };
        var baseArmDefinition = new GeometryFileDefinition
        {
            GeometryId = "base-arm-con",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseArmFileName, cache.Geometries["base-arm-con"], NullLogger.Instance),
            FileName = baseArmFileName
        };
        var armDefinition = new GeometryFileDefinition
        {
            GeometryId = "arm",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(armFileName, cache.Geometries["arm"], NullLogger.Instance),
            FileName = armFileName
        };
        var armHeadDefinition = new GeometryFileDefinition
        {
            GeometryId = "arm-head-con",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(armHeadFileName, cache.Geometries["arm-head-con"], NullLogger.Instance),
            FileName = armHeadFileName
        };
        var headDefinition = new GeometryFileDefinition
        {
            GeometryId = "head",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(headFileName, cache.Geometries["head"], NullLogger.Instance),
            FileName = headFileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
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
                GeometryReference = baseDefinition,
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
                                GeometryReference = baseArmDefinition,
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
                                                GeometryReference = armDefinition,
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
                                                                GeometryReference = armHeadDefinition,
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
                                                                                GeometryReference = headDefinition,
                                                                                Position = new Vector3
                                                                                {
                                                                                    X = 0f,
                                                                                    Y = -0.05f,
                                                                                    Z = -0.525f
                                                                                },
                                                                                IncludedInMeasurement = false,
                                                                                LightEmittingObjects = new List<LightEmittingPart>
                                                                                {
                                                                                    new(new Rectangle
                                                                                    {
                                                                                        SizeX = 0.08,
                                                                                        SizeY = 0.18
                                                                                    })
                                                                                    {
                                                                                        Name = "leo",
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

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample003(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_003");
        var objPath = Path.Combine(exampleDirectory, "luminaire", "luminaire.obj");
        var cache = exampleDirectory.ToCache();
        var fileName = Path.GetFileName(objPath);

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0"
        };

        var bodyDefinition = new GeometryFileDefinition
        {
            GeometryId = "luminaire",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(fileName, cache.Geometries["luminaire"], NullLogger.Instance),
            FileName = fileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
        {
            bodyDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "luminaire",
                GeometryReference = bodyDefinition,
                LightEmittingObjects = new List<LightEmittingPart>
                {
                    new(new Circle
                    {
                        Diameter = 0.2
                    })
                    {
                        Name = "leo"
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

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample004(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_004");
        var objPath = Path.Combine(exampleDirectory, "luminaire", "luminaire.obj");
        var cache = exampleDirectory.ToCache();
        var fileName = Path.GetFileName(objPath);

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0"
        };

        var bodyDefinition = new GeometryFileDefinition
        {
            GeometryId = "luminaire",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(fileName, cache.Geometries["luminaire"], NullLogger.Instance),
            FileName = fileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
        {
            bodyDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "luminaire",
                GeometryReference = bodyDefinition,
                Position = new Vector3
                {
                    X = 0f,
                    Y = 0f,
                    Z = -0.5f
                },
                LightEmittingObjects = new List<LightEmittingPart>
                {
                    new(new Rectangle
                    {
                        SizeX = 0.15,
                        SizeY = 1.0
                    })
                    {
                        Name = "leo_top",
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
                    new(new Rectangle
                    {
                        SizeX = 0.17,
                        SizeY = 1.175
                    })
                    {
                        Name = "leo_bottom",
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

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample005(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_005");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");
        var cache = exampleDirectory.ToCache();
        var baseFileName = Path.GetFileName(baseObjPath);
        var headFileName = Path.GetFileName(headObjPath);

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0",
            Name = "Another example",
            Description = "Example luminaire 4"
        };

        var baseDefinition = new GeometryFileDefinition
        {
            GeometryId = "base",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseFileName, cache.Geometries["base"], NullLogger.Instance),
            FileName = baseFileName
        };
        var headDefinition = new GeometryFileDefinition
        {
            GeometryId = "head",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(headFileName, cache.Geometries["head"], NullLogger.Instance),
            FileName = headFileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
        {
            baseDefinition,
            headDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "base",
                GeometryReference = baseDefinition,
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
                                GeometryReference = headDefinition,
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
                                    new(new Rectangle
                                    {
                                        SizeX = 0.15,
                                        SizeY = 0.035
                                    })
                                    {
                                        Name = "leo",
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

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample006(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_006");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var baseHeadConObjPath = Path.Combine(exampleDirectory, "base-head-con", "base-head-con.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");
        var cache = exampleDirectory.ToCache();
        var baseFileName = Path.GetFileName(baseObjPath);
        var baseHeadConFileName = Path.GetFileName(baseHeadConObjPath);
        var headFileName = Path.GetFileName(headObjPath);

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Experimental"
        };

        var baseDefinition = new GeometryFileDefinition
        {
            GeometryId = "base",
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(baseFileName, cache.Geometries["base"], NullLogger.Instance),
            FileName = baseFileName
        };
        var baseHeadConnectionDefinition = new GeometryFileDefinition
        {
            GeometryId = "base-head-con",
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(baseHeadConFileName, cache.Geometries["base-head-con"], NullLogger.Instance),
            FileName = baseHeadConFileName
        };
        var headDefinition = new GeometryFileDefinition
        {
            GeometryId = "head",
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(headFileName, cache.Geometries["head"], NullLogger.Instance),
            FileName = headFileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
        {
            baseDefinition,
            baseHeadConnectionDefinition,
            headDefinition
        };

        GeometryPart CreatePart(int index, float yPosition) => new()
        {
            Name = $"part-{index}",
            GeometryReference = baseHeadConnectionDefinition,
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
                            GeometryReference = headDefinition,
                            LightEmittingObjects = new List<LightEmittingPart>
                            {
                                new(new Circle
                                {
                                    Diameter = 0.1
                                })
                                {
                                    Name = $"leo-{index}",
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
                GeometryReference = baseDefinition,
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

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample007(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_007");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var baseHeadConObjPath = Path.Combine(exampleDirectory, "base-head-con", "base-head-con.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");
        var cache = exampleDirectory.ToCache();
        var baseFileName = Path.GetFileName(baseObjPath);
        var baseHeadConFileName = Path.GetFileName(baseHeadConObjPath);
        var headFileName = Path.GetFileName(headObjPath);

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Experimental"
        };

        var baseDefinition = new GeometryFileDefinition
        {
            GeometryId = "base",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseFileName, cache.Geometries["base"], NullLogger.Instance),
            FileName = baseFileName
        };
        var baseHeadConnectionDefinition = new GeometryFileDefinition
        {
            GeometryId = "base-head-con",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(baseHeadConFileName, cache.Geometries["base-head-con"], NullLogger.Instance),
            FileName = baseHeadConFileName
        };
        var headDefinition = new GeometryFileDefinition
        {
            GeometryId = "head",
            Units = GeometricUnits.mm,
            Model = ObjParser.Parse(headFileName, cache.Geometries["head"], NullLogger.Instance),
            FileName = headFileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
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
                GeometryReference = baseDefinition,
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
                                GeometryReference = baseHeadConnectionDefinition,
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
                                                GeometryReference = headDefinition,
                                                Position = new Vector3
                                                {
                                                    X = 0.4f,
                                                    Y = 0f,
                                                    Z = -0.0375f
                                                },
                                                LightEmittingObjects = new List<LightEmittingPart>
                                                {
                                                    new(new Circle
                                                    {
                                                        Diameter = 0.0575
                                                    })
                                                    {
                                                        Name = "LEO0",
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
                                GeometryReference = baseHeadConnectionDefinition,
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
                                                GeometryReference = headDefinition,
                                                Position = new Vector3
                                                {
                                                    X = 0.4f,
                                                    Y = 0f,
                                                    Z = -0.0375f
                                                },
                                                LightEmittingObjects = new List<LightEmittingPart>
                                                {
                                                    new(new Circle
                                                    {
                                                        Diameter = 0.0575
                                                    })
                                                    {
                                                        Name = "LEO1",
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

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample008(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_008");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "textured_cube.obj");
        var cache = exampleDirectory.ToCache();
        var fileName = Path.GetFileName(cubeObjPath);

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var baseDefinition = new GeometryFileDefinition
        {
            GeometryId = "cube",
            Units = GeometricUnits.m,
            Model = ObjParser.Parse(fileName, cache.Geometries["cube"], NullLogger.Instance),
            FileName = fileName
        };

        luminaire.GeometryDefinitions = new List<GeometryFileDefinition>
        {
            baseDefinition
        };

        luminaire.Parts = new List<GeometryPart>
        {
            new()
            {
                Name = "luminaire",
                GeometryReference = baseDefinition,
                LightEmittingObjects = new List<LightEmittingPart>
                {
                    new(new Rectangle
                    {
                        SizeX = 0.5,
                        SizeY = 0.25
                    })
                    {
                        Name = "leo"
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

        return Resolver.Resolve(luminaire, cache)!;
    }
}