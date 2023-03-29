# L3D .NET library

[![OnPublishedRelease](https://github.com/globallightingdata/l3d.net/actions/workflows/OnPublishedRelease.yml/badge.svg)](https://github.com/globallightingdata/l3d.net/actions/workflows/OnPublishedRelease.yml)  
[![OnPush develop](https://github.com/globallightingdata/l3d.net/actions/workflows/OnPushDevelop.yml/badge.svg)](https://github.com/globallightingdata/l3d.net/actions/workflows/OnPushDevelop.yml)  
[![NuGet Status](https://img.shields.io/nuget/v/L3D.Net.svg)](https://www.nuget.org/packages/L3D.Net/) [![L3D.Net on fuget.org](https://www.fuget.org/packages/L3D.Net/badge.svg)](https://www.fuget.org/packages/L3D.Net)


## Intro

.NET Standard 2.0 library for the Luminaire 3D [L3D](https://gldf.io/docs/geometry/l3d-intro)

With the library it is possible to read and build L3D container files. For that the library exposes two classes 'Builder' and 'Reader'.
The Builder has a fluent API for defining all the luminaire parts and build the target container file.
With the Reader is is possible to read the content of a L3D container and to parse the containing .obj files at once. So there is no other .obj parser needed.

## How to get started

### Requirements

- [.NET Standard 2.0](https://docs.microsoft.com/de-de/dotnet/standard/net-standard) compatible project

### Nuget package

Add the package within your IDE or using the CLI

```bash
dotnet add package L3D.Net
```

---

### Building a L3D container

Simple builder exmaple.

```CSharp
var luminaire = new Luminaire();
luminaire.Header = new Header
{
    CreatedWithApplication = "Example-Tool"
};

var geometryDefinition = new GeometryFileDefinition
{
    GeometryId = "PN." + Guid.NewGuid(),
    Units = GeometricUnits.m,
    Model = ObjParser.Parse(cubeObjPath, NullLogger.Instance),
    FileName = cubeObjPath
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
        GeometryReference = geometryDefinition
    }
};

IWriter writer = new Writer();
var bytes = writer.WriteToByteArray(luminaire);
```

### Reading a L3D container

Read an L3D container from disk:
```CSharp
var reader = new Reader();
var container = reader.ReadContainer("path/to/container.l3d");

foreach (var geometryPartDto in container.Parts)
{
    // ...
    var model = geometryPartDto.GeometryDefinition.Model;
    foreach (var vertex in model.Vertices)
    {
        // .obj-Model vertices
    }

    // ...
    
    foreach (var lightEmittingPartDto in geometryPartDto.LightEmittingObjects)
    {
        // do something for light emitting part 
    }

    // ...
}
```

Read an L3D container already read from disk in byte array (byte[]) format:
```CSharp
var reader = new Reader();
byte[] l3dContainer = // Get a byte[] representation of an L3D container;
var container = reader.ReadContainer(l3dContainer);

foreach (var geometryPartDto in container.Parts)
{
    // From here on out everything is identical to the reading an L3D container from disk example,
    // see the code sample above.
}
```

### Validating a L3D container

Validation of an L3D container from disk:
```CSharp
var validator = new Validator();
var validationResult = validator.ValidateContainer("path/to/container.l3d");

if (validationResult)
    // Container validation successful
else
    // Container validation unsuccessful
```

Validation of an L3D container already read from disk in byte array (byte[]) format:
```CSharp
var validator = new Validator();
byte[] l3dContainer = // Get a byte[] representation of an L3D container;
var validationResult = validator.ValidateContainer(l3dContainer);

if (validationResult)
    // Container validation successful
else
    // Container validation unsuccessful
```

### Compatibility

L3D.NET is able to read old and new unknown versions, with the exception of a new major version.

When a new minor version is read, it is possible that optional information is not read.
If the L3D is saved again, this information is not written either and the version is set to the latest version of the L3D.NET component.

The following table explains this behaviour summarized:

File version | L3D.NET version | Can read | Can write | Read and write changes the file
------------ | --------------- | -------- | --------- | ----------------------------
1.0.0.0      | 0.9.0.0         | no       | no        | N/A
0.9.0.0      | 1.0.0.0         | yes      | yes*      | yes - the version is updated and the content accordingly too
1.0.0.0      | 1.0.0.0         | yes      | yes       | no
0.9.2.0      | 0.9.0.0         | yes*     | yes*      | yes - some optional information may not be read or written
0.9.0.0      | 0.9.2.0         | yes      | yes*      | yes - the version is updated and the content accordingly too

---

## Questions, Issues & Contribution

Please use the discussion section for questions or create issues, when something seems to be wrong. PRs are welcome.

