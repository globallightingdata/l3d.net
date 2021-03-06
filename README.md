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
Builder.NewLuminaire()
    .WithTool("Example-Tool")
    .AddGeometry("luminairePartName", "path/to/model.obj", GeometricUnits.m, geomOptions => geomOptions
        .AddRectangularLightEmittingObject("lightEmittingPartName", 0.5, 0.25, leoOptions => leoOptions
            .WithLightEmittingSurfaceOnParent(3)
        )
    )
    .Build("path/to/new/container.l3d");
```

### Reading a L3D container

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

---

## Questions, Issues & Contribution

Please use the discussion section for questions or create issues, when something seems to be wrong. PRs are welcome.
