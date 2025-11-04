using System.Collections.Generic;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UnusedMethodReturnValue.Local

namespace L3D.Net.Tests.Internal;

[TestFixture]
public partial class ContainerValidatorTests
{
    #region Validate

    private void MockLuminaireWithMissingReferences(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new GeometryPart
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new JointPart
                        {
                            Geometries =
                            [
                                new GeometryPart
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2"
                                    }
                                },

                                new GeometryPart()
                            ]
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireIsNull(Context context)
    {
        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns((Luminaire) null!);
    }

    private void MockLuminaireHasUnusedFiles(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.Files.Returns(new Dictionary<string, FileInformation>());

        var model3 = Substitute.For<IModel3D>();
        model3.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>
        {
            ["mtl1"] = []
        });
        model3.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>
        {
            ["tex1"] = []
        });
        model3.Files.Returns(new Dictionary<string, FileInformation>());

        var model4 = Substitute.For<IModel3D>();
        model4.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model4.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model4.Files.Returns(new Dictionary<string, FileInformation>());

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new GeometryPart
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    },
                    Joints =
                    [
                        new JointPart
                        {
                            Geometries =
                            [
                                new GeometryPart
                                {
                                    GeometryReference = new GeometryFileDefinition
                                    {
                                        GeometryId = "id2"
                                    }
                                },

                                new GeometryPart()
                            ]
                        }
                    ]
                }
            ],
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    GeometryId = "id1",
                    Model = model1
                },

                new GeometryFileDefinition
                {
                    GeometryId = "id3",
                    Model = model3
                },

                new GeometryFileDefinition
                {
                    GeometryId = "id4",
                    Model = model4,
                    FileName = "obj1"
                }
            ]
        });
    }

    private void MockLuminaireHasUnusedFilesUsingFilesDictionary(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.Files.Returns(new Dictionary<string, FileInformation>
        {
            ["mtl"] = new() {Name = "mtl1", Status = FileStatus.Unused}
        });
        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new GeometryPart
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    }
                }
            ],
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    GeometryId = "id1",
                    Model = model1
                }
            ]
        });
    }

    private void MockLuminaireHasMissingMaterialUsingFilesDictionary(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.Files.Returns(new Dictionary<string, FileInformation>
        {
            ["mtl1"] = new() {Status = FileStatus.MissingMaterial}
        });

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new GeometryPart
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    }
                }
            ],
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    GeometryId = "id1",
                    Model = model1
                }
            ]
        });
    }

    private void MockLuminaireHasMissingTextureUsingFilesDictionary(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.Files.Returns(new Dictionary<string, FileInformation>
        {
            ["tex1"] = new() {Status = FileStatus.MissingTexture}
        });

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Parts =
            [
                new GeometryPart
                {
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "id1"
                    }
                }
            ],
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    GeometryId = "id1",
                    Model = model1
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidCreatedWithApplication(Context context, string? value)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = value!
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasDuplicatedPartNames(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "leoPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasNoGeometryDefinitions(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasNoParts(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ]
        });
    }

    private void MockLuminaireHasNoLightEmittingPart(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    }
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidPartNames(Context context, string? invalid)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "1geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "le"
                        },

                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = invalid!
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "leoPart%",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["le"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new JointPart
                        {
                            Name = "j"
                        }
                    ],
                    Sensors =
                    [
                        new SensorPart
                        {
                            Name = "%%%"
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidGroupIndex(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = -1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidFaceIndex(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = -1
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidFaceReference(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 2,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidFaceIndexBegin(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new FaceRangeAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndexBegin = -1
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidFaceIndexEnd(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new FaceRangeAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndexBegin = 2,
                                    FaceIndexEnd = 1
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidFaceReferences(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new FaceRangeAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndexBegin = 2,
                                    FaceIndexEnd = 3
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidNameReference(Context context, string? invalid)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                [invalid ?? string.Empty] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidIntensities(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart1",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = -0.1
                            }
                        },

                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart2",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 1.1
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidAxisRotations(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new JointPart
                        {
                            Name = "joint",
                            XAxis = new AxisRotation
                            {
                                Max = 0,
                                Min = 0,
                                Step = 0,
                            },
                            YAxis = new AxisRotation
                            {
                                Max = 1,
                                Min = 2,
                                Step = -1,
                            },
                            ZAxis = new AxisRotation
                            {
                                Max = -2,
                                Min = -1,
                                Step = -2,
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasJointWithoutGeometries(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ],
                    Joints =
                    [
                        new JointPart
                        {
                            Name = "joint"
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidLightEmittingPartShapes(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = -0.1
                        })
                        {
                            Name = "leoPart1"
                        },

                        new LightEmittingPart(new Rectangle
                        {
                            SizeX = -0.1,
                            SizeY = -0.1
                        })
                        {
                            Name = "leoPart2"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasInvalidGeometryReference(Context context, string? invalid)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = invalid!,
                        FileName = invalid!,
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MockLuminaireHasNoModel(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.FileName.Returns("file.obj");
        model1.Data.Returns(new ModelData());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    private void MocknLuminaireHasNoModelData(Context context)
    {
        var model1 = Substitute.For<IModel3D>();
        model1.ReferencedMaterialLibraryFiles.Returns(new Dictionary<string, byte[]>());
        model1.ReferencedTextureFiles.Returns(new Dictionary<string, byte[]>());
        model1.IsFaceIndexValid(1, 2).Returns(true);

        context.L3DXmlReader.Read(Arg.Any<ContainerCache>()).Returns(new Luminaire
        {
            Header = new Header
            {
                CreatedWithApplication = "test"
            },
            GeometryDefinitions =
            [
                new GeometryFileDefinition
                {
                    Model = model1,
                    GeometryId = "geo",
                    FileName = "file.obj",
                    Units = GeometricUnits.m
                }
            ],
            Parts =
            [
                new GeometryPart
                {
                    Name = "geoPart",
                    GeometryReference = new GeometryFileDefinition
                    {
                        Model = model1,
                        GeometryId = "geo",
                        FileName = "file.obj",
                        Units = GeometricUnits.m
                    },
                    LightEmittingObjects =
                    [
                        new LightEmittingPart(new Circle
                        {
                            Diameter = 0.1
                        })
                        {
                            Name = "leoPart"
                        }
                    ],
                    LightEmittingSurfaces =
                    [
                        new LightEmittingSurfacePart
                        {
                            Name = "lesPart",
                            FaceAssignments =
                            [
                                new SingleFaceAssignment
                                {
                                    GroupIndex = 1,
                                    FaceIndex = 2
                                }
                            ],
                            LightEmittingPartIntensityMapping = new Dictionary<string, double>
                            {
                                ["leoPart"] = 0.8
                            }
                        }
                    ]
                }
            ]
        });
    }

    #endregion
}