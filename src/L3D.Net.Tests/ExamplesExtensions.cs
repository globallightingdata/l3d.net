using L3D.Net.Data;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using L3D.Net.Geometry;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_11_0;

namespace L3D.Net.Tests;

public static class ExamplesExtensions
{
    private static readonly ObjParser ObjParser = ObjParser.Instance;
    private static readonly LuminaireResolver Resolver = LuminaireResolver.Instance;

    private static GeometryFileDefinition CreateFileDefinition(string id, string objPath, GeometricUnits unit, ContainerCache cache)
    {
        var filename = Path.GetFileName(objPath);
        var model = ObjParser.Parse(filename, cache.Geometries[id], NullLogger.Instance);
        var fileDefinition = new GeometryFileDefinition
        {
            GeometryId = id,
            Units = unit,
            Model = model,
            FileName = filename
        };
        LuminaireResolver.ScaleModel(model!, LuminaireResolver.GetScale(unit));
        Resolver.ResolveModelMaterials(model!, id, cache);
        return fileDefinition;
    }

    public static Luminaire BuildExample000(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_000");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "cube.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var geometryDefinition = CreateFileDefinition("cube", cubeObjPath, GeometricUnits.m, cache);

        luminaire.GeometryDefinitions = [geometryDefinition];

        luminaire.Parts =
        [
            new()
            {
                Name = "luminaire",
                LightEmittingObjects =
                [
                    new(new Rectangle
                    {
                        SizeX = 0.5,
                        SizeY = 0.25
                    })
                    {
                        Name = "leo"
                    }
                ],
                LightEmittingSurfaces =
                [
                    new()
                    {
                        Name = "les",
                        FaceAssignments =
                        [
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                ],
                GeometryReference = geometryDefinition
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample001(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_001");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "cube.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var geometryDefinition = CreateFileDefinition("cube", cubeObjPath, GeometricUnits.m, cache);

        luminaire.GeometryDefinitions = [geometryDefinition];

        luminaire.Parts =
        [
            new()
            {
                Name = "luminaire",
                LightEmittingObjects =
                [
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
                ],
                LightEmittingSurfaces =
                [
                    new()
                    {
                        Name = "les",
                        FaceAssignments =
                        [
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                ],
                Position = new Vector3
                {
                    X = -0.25f,
                    Y = -0.125f,
                    Z = 0.05f
                },
                GeometryReference = geometryDefinition
            }
        ];

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

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0",
            Name = "First example",
            Description = "First ever xml luminaire geometry description"
        };

        var baseDefinition = CreateFileDefinition("base", baseObjPath, GeometricUnits.mm, cache);
        var baseArmDefinition = CreateFileDefinition("base-arm-con", baseArmConObjPath, GeometricUnits.mm, cache);
        var armDefinition = CreateFileDefinition("arm", armObjPath, GeometricUnits.mm, cache);
        var armHeadDefinition = CreateFileDefinition("arm-head-con", armHeadConObjPath, GeometricUnits.mm, cache);
        var headDefinition = CreateFileDefinition("head", headObjPath, GeometricUnits.mm, cache);

        luminaire.GeometryDefinitions =
        [
            baseDefinition,
            baseArmDefinition,
            armDefinition,
            armHeadDefinition,
            headDefinition
        ];

        luminaire.Parts =
        [
            new()
            {
                Name = "base",
                GeometryReference = baseDefinition,
                Joints =
                [
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
                        Geometries =
                        [
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
                                Joints =
                                [
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
                                        Geometries =
                                        [
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
                                                Joints =
                                                [
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
                                                        Geometries =
                                                        [
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
                                                                Joints =
                                                                [
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
                                                                        Geometries =
                                                                        [
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
                                                                                LightEmittingObjects =
                                                                                [
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
                                                                                ],
                                                                                LightEmittingSurfaces =
                                                                                [
                                                                                    new()
                                                                                    {
                                                                                        Name = "les",
                                                                                        FaceAssignments =
                                                                                        [
                                                                                            new FaceRangeAssignment
                                                                                            {
                                                                                                FaceIndexBegin = 16,
                                                                                                FaceIndexEnd = 21
                                                                                            }
                                                                                        ],
                                                                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                                                                        {
                                                                                            ["leo"] = 1
                                                                                        }
                                                                                    }
                                                                                ]
                                                                            }
                                                                        ]
                                                                    }
                                                                ]
                                                            }
                                                        ]
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample003(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_003");
        var objPath = Path.Combine(exampleDirectory, "luminaire", "luminaire.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0"
        };


        var bodyDefinition = CreateFileDefinition("luminaire", objPath, GeometricUnits.mm, cache);

        luminaire.GeometryDefinitions = [bodyDefinition];

        luminaire.Parts =
        [
            new()
            {
                Name = "luminaire",
                GeometryReference = bodyDefinition,
                LightEmittingObjects =
                [
                    new(new Circle
                    {
                        Diameter = 0.2
                    })
                    {
                        Name = "leo"
                    }
                ],
                LightEmittingSurfaces =
                [
                    new()
                    {
                        Name = "les",
                        FaceAssignments =
                        [
                            new FaceRangeAssignment
                            {
                                FaceIndexBegin = 1199,
                                FaceIndexEnd = 1235
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample004(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_004");
        var objPath = Path.Combine(exampleDirectory, "luminaire", "luminaire.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0"
        };

        var bodyDefinition = CreateFileDefinition("luminaire", objPath, GeometricUnits.mm, cache);

        luminaire.GeometryDefinitions = [bodyDefinition];

        luminaire.Parts =
        [
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
                LightEmittingObjects =
                [
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
                ],
                LightEmittingSurfaces =
                [
                    new()
                    {
                        Name = "les_top",
                        FaceAssignments =
                        [
                            new FaceRangeAssignment
                            {
                                FaceIndexBegin = 84,
                                FaceIndexEnd = 85
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo_top"] = 1
                        }
                    },

                    new()
                    {
                        Name = "les_bottom",
                        FaceAssignments =
                        [
                            new FaceRangeAssignment
                            {
                                FaceIndexBegin = 90,
                                FaceIndexEnd = 91
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo_bottom"] = 1
                        }
                    }
                ],
                ElectricalConnectors =
                [
                    new()
                    {
                        X = -0.575f,
                        Y = 0f,
                        Z = 0.04f
                    }
                ],
                PendulumConnectors =
                [
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
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample005(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_005");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Keyboard-v1.0",
            Name = "Another example",
            Description = "Example luminaire 4"
        };

        var baseDefinition = CreateFileDefinition("base", baseObjPath, GeometricUnits.mm, cache);
        var headDefinition = CreateFileDefinition("head", headObjPath, GeometricUnits.mm, cache);

        luminaire.GeometryDefinitions =
        [
            baseDefinition,
            headDefinition
        ];

        luminaire.Parts =
        [
            new()
            {
                Name = "base",
                GeometryReference = baseDefinition,
                Joints =
                [
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
                        Geometries =
                        [
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
                                LightEmittingObjects =
                                [
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
                                ],
                                LightEmittingSurfaces =
                                [
                                    new()
                                    {
                                        Name = "les",
                                        FaceAssignments =
                                        [
                                            new FaceRangeAssignment
                                            {
                                                FaceIndexBegin = 158,
                                                FaceIndexEnd = 179
                                            }
                                        ],
                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                        {
                                            ["leo"] = 1
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample006(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_006");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var baseHeadConObjPath = Path.Combine(exampleDirectory, "base-head-con", "base-head-con.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Experimental"
        };

        var baseDefinition = CreateFileDefinition("base", baseObjPath, GeometricUnits.m, cache);
        var baseHeadConnectionDefinition = CreateFileDefinition("base-head-con", baseHeadConObjPath, GeometricUnits.m, cache);
        var headDefinition = CreateFileDefinition("head", headObjPath, GeometricUnits.m, cache);

        luminaire.GeometryDefinitions =
        [
            baseDefinition,
            baseHeadConnectionDefinition,
            headDefinition
        ];

        GeometryPart CreatePart(int index, float yPosition) => new()
        {
            Name = $"part-{index}",
            GeometryReference = baseHeadConnectionDefinition,
            Joints =
            [
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
                    Geometries =
                    [
                        new()
                        {
                            Name = $"head-{index}",
                            GeometryReference = headDefinition,
                            LightEmittingObjects =
                            [
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
                            ],
                            LightEmittingSurfaces =
                            [
                                new()
                                {
                                    Name = $"les-{index}",
                                    FaceAssignments =
                                    [
                                        new FaceRangeAssignment
                                        {
                                            FaceIndexBegin = 50,
                                            FaceIndexEnd = 97
                                        }
                                    ],
                                    LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                    {
                                        [$"leo-{index}"] = 1
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        luminaire.Parts =
        [
            new()
            {
                Name = "base",
                GeometryReference = baseDefinition,
                Joints =
                [
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
                        Geometries =
                        [
                            CreatePart(0, -0.55f),
                            CreatePart(1, -0.15f),
                            CreatePart(2, 0.25f),
                            CreatePart(3, 0.65f)
                        ]
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample007(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_007");
        var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
        var baseHeadConObjPath = Path.Combine(exampleDirectory, "base-head-con", "base-head-con.obj");
        var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Experimental"
        };

        var baseDefinition = CreateFileDefinition("base", baseObjPath, GeometricUnits.m, cache);
        var baseHeadConnectionDefinition = CreateFileDefinition("base-head-con", baseHeadConObjPath, GeometricUnits.m, cache);
        var headDefinition = CreateFileDefinition("head", headObjPath, GeometricUnits.m, cache);

        luminaire.GeometryDefinitions =
        [
            baseDefinition,
            baseHeadConnectionDefinition,
            headDefinition
        ];

        luminaire.Parts =
        [
            new()
            {
                Name = "base",
                GeometryReference = baseDefinition,
                Joints =
                [
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
                        Geometries =
                        [
                            new()
                            {
                                Name = "base-head-con-0",
                                GeometryReference = baseHeadConnectionDefinition,
                                Joints =
                                [
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
                                        Geometries =
                                        [
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
                                                LightEmittingObjects =
                                                [
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
                                                ],
                                                LightEmittingSurfaces =
                                                [
                                                    new()
                                                    {
                                                        Name = "LES0",
                                                        FaceAssignments =
                                                        [
                                                            new FaceRangeAssignment
                                                            {
                                                                FaceIndexBegin = 574,
                                                                FaceIndexEnd = 607
                                                            }
                                                        ],
                                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                                        {
                                                            ["LEO0"] = 1
                                                        }
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
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
                        Geometries =
                        [
                            new()
                            {
                                Name = "base-head-con-1",
                                GeometryReference = baseHeadConnectionDefinition,
                                Joints =
                                [
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
                                        Geometries =
                                        [
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
                                                LightEmittingObjects =
                                                [
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
                                                ],
                                                LightEmittingSurfaces =
                                                [
                                                    new()
                                                    {
                                                        Name = "LES1",
                                                        FaceAssignments =
                                                        [
                                                            new FaceRangeAssignment
                                                            {
                                                                FaceIndexBegin = 574,
                                                                FaceIndexEnd = 607
                                                            }
                                                        ],
                                                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                                                        {
                                                            ["LEO1"] = 1
                                                        }
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ],
                Sensors =
                [
                    new()
                    {
                        Name = "Sensor"
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample008(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_008");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "textured_cube.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var baseDefinition = CreateFileDefinition("cube", cubeObjPath, GeometricUnits.m, cache);

        luminaire.GeometryDefinitions = [baseDefinition];

        luminaire.Parts =
        [
            new()
            {
                Name = "luminaire",
                GeometryReference = baseDefinition,
                LightEmittingObjects =
                [
                    new(new Rectangle
                    {
                        SizeX = 0.5,
                        SizeY = 0.25
                    })
                    {
                        Name = "leo"
                    }
                ],
                LightEmittingSurfaces =
                [
                    new()
                    {
                        Name = "les",
                        FaceAssignments =
                        [
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample009(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_009");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "textured_cube.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };

        var baseDefinition = CreateFileDefinition("cube", cubeObjPath, GeometricUnits.m, cache);

        luminaire.GeometryDefinitions = [baseDefinition];

        luminaire.Parts =
        [
            new()
            {
                Name = "luminaire",
                GeometryReference = baseDefinition,
                LightEmittingObjects =
                [
                    new(new Rectangle
                    {
                        SizeX = 0.5,
                        SizeY = 0.25
                    })
                    {
                        Name = "leo"
                    }
                ],
                LightEmittingSurfaces =
                [
                    new()
                    {
                        Name = "les",
                        FaceAssignments =
                        [
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample010(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_010");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "textured_cube.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };
        var baseDefinition = CreateFileDefinition("cube", cubeObjPath, GeometricUnits.m, cache);

        luminaire.GeometryDefinitions = [baseDefinition];

        luminaire.Parts =
        [
            new()
            {
                Name = "luminaire",
                GeometryReference = baseDefinition,
                LightEmittingObjects =
                [
                    new(new Rectangle
                    {
                        SizeX = 0.5,
                        SizeY = 0.25
                    })
                    {
                        Name = "leo"
                    }
                ],
                LightEmittingSurfaces =
                [
                    new()
                    {
                        Name = "les",
                        FaceAssignments =
                        [
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }

    public static Luminaire BuildExample011(this Luminaire luminaire)
    {
        var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_011");
        var cubeObjPath = Path.Combine(exampleDirectory, "cube", "textured_cube.obj");
        var cache = exampleDirectory.ToCache();

        luminaire.Header = new Header
        {
            CreatedWithApplication = "Example-Tool"
        };
        var baseDefinition = CreateFileDefinition("cube", cubeObjPath, GeometricUnits.m, cache);

        luminaire.GeometryDefinitions = [baseDefinition];

        luminaire.Parts =
        [
            new()
            {
                Name = "luminaire",
                GeometryReference = baseDefinition,
                LightEmittingObjects =
                [
                    new(new Rectangle
                    {
                        SizeX = 0.5,
                        SizeY = 0.25
                    })
                    {
                        Name = "leo"
                    }
                ],
                LightEmittingSurfaces =
                [
                    new()
                    {
                        Name = "les",
                        FaceAssignments =
                        [
                            new SingleFaceAssignment
                            {
                                FaceIndex = 3
                            }
                        ],
                        LightEmittingPartIntensityMapping = new Dictionary<string, double>
                        {
                            ["leo"] = 1
                        }
                    }
                ]
            }
        ];

        return Resolver.Resolve(luminaire, cache)!;
    }
}