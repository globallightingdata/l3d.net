using L3D.Net.Data;
using L3D.Net.Geometry;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace L3D.Net.XML.V0_11_0
{
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

        public Luminaire Resolve(Luminaire luminaire, string workingDirectory, ILogger? logger = null)
        {
            var geometryParts = luminaire.Parts.SelectMany(GetParts);

            foreach (var geometryPart in geometryParts)
            {
                geometryPart.GeometryReference = ResolveGeometrySource(geometryPart.GeometryReference, luminaire.GeometryDefinitions, workingDirectory, logger);
            }

            return luminaire;
        }

        private GeometryFileDefinition? ResolveGeometrySource(GeometryFileDefinition? geometryFileDefinition, IEnumerable<GeometryFileDefinition> geometrySources, string workingDirectory, ILogger? logger)
        {
            if (geometryFileDefinition == null)
                return null;

            var source = geometrySources.FirstOrDefault(x => x.GeometryId.Equals(geometryFileDefinition.GeometryId, StringComparison.Ordinal));

            if (source == null)
                return geometryFileDefinition;

            var modelPath = Path.Combine(workingDirectory, source.GeometryId, source.FileName);

            var model = _objParser.Parse(modelPath, logger);

            geometryFileDefinition.Model = ScaleModel(model, GetScale(geometryFileDefinition.Units), source.GeometryId, workingDirectory);
            geometryFileDefinition.Units = source.Units;
            geometryFileDefinition.FileName = source.FileName;

            return geometryFileDefinition;
        }

        private IModel3D ScaleModel(IModel3D model, float scale, string geomId, string directory)
        {
            if (model.Data == null)
                return model;

            model.Data.Vertices = model.Data.Vertices.Select(vector3 => vector3 * scale).ToList();
            model.Data.Normals = model.Data.Normals.ToList();
            model.Data.TextureCoordinates = model.Data.TextureCoordinates.ToList();
            model.Data.FaceGroups = model.Data.FaceGroups.ToList();
            model.Data.Materials = model.Data.Materials.Select(material => ResolveMaterial(material, geomId, directory))
                .ToList();

            return model;
        }

        private ModelMaterial ResolveMaterial(ModelMaterial material, string geomId, string directory)
        {
            var textureBytes = Array.Empty<byte>();

            if (!string.IsNullOrWhiteSpace(material.TextureName))
                textureBytes = _fileHandler.GetTextureBytes(directory, geomId, material.TextureName);

            material.TextureBytes = textureBytes;

            return material;
        }


        private static float GetScale(GeometricUnits units) => units switch
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

            foreach (var subGeometryPart in geometryPart.Joints.SelectMany(x => x.Geometries))
            {
                foreach (var part in GetParts(subGeometryPart))
                {
                    yield return part;
                }
            }
        }
    }
}
