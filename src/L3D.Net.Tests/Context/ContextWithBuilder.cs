using System;
using L3D.Net.API.Dto;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using NSubstitute;

namespace L3D.Net.Tests.Context;

internal interface IContextWithBuilder
{
    ILuminaireBuilder LuminaireBuilder { get; }
}

internal interface IContextWithBuilderOptions
{
    IContextWithBuilder Context { get; }
}

internal static class ContextWithBuilderExtensions
{
    public static TOptions WithGeometryDefinition<TOptions>(this TOptions options, string modelPath, GeometricUnits units, out GeometryDefinition geometryDefinition) where TOptions : IContextWithBuilderOptions
    {
        geometryDefinition = new GeometryDefinition(Guid.NewGuid().ToString(), Substitute.For<IModel3D>(), units);
        options.Context.LuminaireBuilder.EnsureGeometryFileDefinition(Arg.Is(modelPath), Arg.Is(GeometricUnits.m)).Returns(geometryDefinition);
        return options;
    }

    public static TOptions WithValidLightEmittingPartName<TOptions>(this TOptions options, string lightEmittingPartName) where TOptions : IContextWithBuilderOptions
    {
        options.Context.LuminaireBuilder.IsValidLightEmittingPartName(Arg.Is(lightEmittingPartName)).Returns(true);
        return options;
    }

}