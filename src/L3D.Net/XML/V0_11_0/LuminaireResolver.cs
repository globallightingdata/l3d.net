using L3D.Net.Data;
using L3D.Net.Geometry;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace L3D.Net.XML.V0_11_0;

internal class LuminaireResolver : ILuminaireResolver
{
    public static readonly LuminaireResolver Instance = new(ObjParser.Instance, FileHandler.Instance);

    private readonly IObjParser _objParser;
    private readonly IFileHandler _fileHandler;

    public LuminaireResolver(IObjParser objParser, IFileHandler fileHandler)
    {
        _objParser = objParser ?? throw new ArgumentNullException(nameof(objParser));
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
    }

    public Luminaire? Resolve(Luminaire? luminaire, ContainerCache cache, ILogger? logger = null)
    {
        if (luminaire == null)
            return null;

        var geometryParts = luminaire.Parts.SelectMany(GetParts);
        foreach (var geometryPart in geometryParts)
        {
            geometryPart.GeometryReference = ResolveGeometrySource(geometryPart.GeometryReference, luminaire.GeometryDefinitions, cache, logger);
        }

        return luminaire;
    }

    private GeometryFileDefinition ResolveGeometrySource(GeometryFileDefinition geometryFileDefinition, List<GeometryFileDefinition> geometrySources, ContainerCache cache,
        ILogger? logger)
    {
        var source = geometrySources.Find(x => x.GeometryId.Equals(geometryFileDefinition.GeometryId, StringComparison.Ordinal));

        if (source == null)
            return geometryFileDefinition;

        if (source.Model is not null || !cache.Geometries.TryGetValue(source.GeometryId, out var files))
            return source;

        source.Model = _objParser.Parse(source.FileName, files!, logger);

        if (source.Model is null) return source;

        ScaleModel(source.Model, GetScale(source.Units));
        ResolveModelMaterials(source.Model, source.GeometryId, cache);

        return source;
    }

    internal static void ScaleModel(IModel3D model, float scale)
    {
        if (model.Data == null) return;

        for (var i = 0; i < model.Data.Vertices.Count; i++)
            model.Data.Vertices[i] *= scale;
    }

    internal void ResolveModelMaterials(IModel3D model, string geomId, ContainerCache cache)
    {
        if (model.Data == null) return;

        foreach (var material in model.Data.Materials)
        {
            ResolveMaterial(material, geomId, cache);
        }
    }

    private void ResolveMaterial(ModelMaterial material, string geomId, ContainerCache cache)
    {
        var textureBytes = Array.Empty<byte>();

        if (!string.IsNullOrWhiteSpace(material.TextureName))
            textureBytes = _fileHandler.GetTextureBytes(cache, geomId, material.TextureName);

        material.TextureBytes = textureBytes;
    }


    internal static float GetScale(GeometricUnits units) => units switch
    {
        GeometricUnits.m => 1f,
        GeometricUnits.dm => 0.1f,
        GeometricUnits.cm => 0.01f,
        GeometricUnits.mm => 0.001f,
        GeometricUnits.yard => 0.9144f,
        GeometricUnits.foot => 0.3048f,
        GeometricUnits.inch => 0.0254f,
        _ => throw new ArgumentOutOfRangeException(nameof(units))
    };

    private static IEnumerable<GeometryPart> GetParts(GeometryPart geometryPart)
    {
        yield return geometryPart;

        if (geometryPart.Joints is null) yield break;

        foreach (var part in geometryPart.Joints.SelectMany(x => x.Geometries).SelectMany(GetParts))
        {
            yield return part;
        }
    }
}