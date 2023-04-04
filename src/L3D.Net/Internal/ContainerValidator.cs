using L3D.Net.Abstract;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace L3D.Net.Internal;

internal class ContainerValidator : IContainerValidator
{
    private readonly IFileHandler _fileHandler;
    private readonly IXmlValidator _xmlValidator;
    private readonly IL3DXmlReader _l3dXmlReader;

    public ContainerValidator(IFileHandler fileHandler, IXmlValidator xmlValidator, IL3DXmlReader l3DXmlReader)
    {
        _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        _xmlValidator = xmlValidator ?? throw new ArgumentNullException(nameof(xmlValidator));
        _l3dXmlReader = l3DXmlReader ?? throw new ArgumentNullException(nameof(l3DXmlReader));
    }

    public IEnumerable<ValidationHint> Validate(string containerPath, Validation flags)
    {
        if (string.IsNullOrWhiteSpace(containerPath))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(containerPath));

        using var cache = _fileHandler.ExtractContainer(containerPath);

        return ValidateCache(cache, flags);
    }

    public IEnumerable<ValidationHint> Validate(byte[] containerBytes, Validation flags)
    {
        if (containerBytes == null || containerBytes.LongLength == 0)
            throw new ArgumentException(@"Value cannot be null or empty array.", nameof(containerBytes));

        using var cache = _fileHandler.ExtractContainer(containerBytes);

        return ValidateCache(cache, flags);
    }

    public IEnumerable<ValidationHint> Validate(Stream containerStream, Validation flags)
    {
        if (containerStream == null || containerStream.Length == 0)
            throw new ArgumentException(@"Value cannot be null or empty array.", nameof(containerStream));

        using var cache = _fileHandler.ExtractContainer(containerStream);

        return ValidateCache(cache, flags);
    }

    private IEnumerable<ValidationHint> ValidateCache(ContainerCache? cache, Validation flags)
    {
        if (cache == null)
        {
            if (flags.HasFlag(Validation.IsZipPackage))
            {
                yield return new InvalidZipValidationHint();
            }
            yield break;
        }

        if (cache.StructureXml == null)
        {
            if (flags.HasFlag(Validation.HasStructureXml))
            {
                yield return new StructureXmlMissingValidationHint();
            }
            yield break;
        }

        var xsdValidationHints = Array.Empty<ValidationHint>();

        if (flags.HasFlag(Validation.IsXmlValid))
        {
            xsdValidationHints = _xmlValidator.ValidateStream(cache.StructureXml).ToArray();

            foreach (var validationHint in xsdValidationHints)
            {
                yield return validationHint;
            }
        }

        if (xsdValidationHints.Any(d => d.Severity == Severity.Error))
            yield break;

        var luminaire = _l3dXmlReader.Read(cache);

        if (flags.HasFlag(Validation.IsProductValid) && luminaire == null)
        {
            yield return new NotAL3DValidationHint();
            yield break;
        }

        if (luminaire == null)
            yield break;

        var geometryParts = luminaire.Parts.SelectMany(GetGeometryParts).ToArray();

        if (flags.HasFlag(Validation.DoesReferencedObjectsExist))
        {
            foreach (var geometryPart in geometryParts)
            {
                if (geometryPart.GeometryReference.Model == null)
                    yield return new MissingGeometryReferenceValidationHint(geometryPart.GeometryReference.GeometryId);
            }
        }

        var geometryReferences = geometryParts.Select(x => x.GeometryReference).Where(x => x != null).ToArray()!;
        IModel3D[] models = geometryReferences.Where(x => x.Model != null).Select(x => x.Model).ToArray()!;
        var mtlNames = models.SelectMany(x => x.ReferencedMaterialLibraryFiles.Keys).ToArray();
        var textureNames = models.SelectMany(x => x.ReferencedTextureFiles.Keys).ToArray();

        var geometryDefinitions = luminaire.GeometryDefinitions;
        IModel3D[] listedModels = geometryDefinitions.Where(x => x.Model != null).Select(x => x.Model).ToArray()!;
        var listedMtlNames = listedModels.SelectMany(x => x.ReferencedMaterialLibraryFiles.Keys).ToArray();
        var listedTextureNames = listedModels.SelectMany(x => x.ReferencedTextureFiles.Keys).ToArray();

        if (flags.HasFlag(Validation.AreAllFileDefinitionsUsed))
        {
            var objFileNames = geometryReferences.Select(x => x.FileName);
            var listedObjNames = geometryDefinitions.Select(x => x.FileName);

            foreach (var objName in listedObjNames.Except(objFileNames))
            {
                yield return new UnusedFileValidationHint(objName);
            }

            foreach (var mtlName in listedMtlNames.Except(mtlNames))
            {
                yield return new UnusedFileValidationHint(mtlName);
            }

            foreach (var textureName in listedTextureNames.Except(textureNames))
            {
                yield return new UnusedFileValidationHint(textureName);
            }
        }

        if (flags.HasFlag(Validation.HasAllMaterials))
        {
            foreach (var mtlName in mtlNames.Except(listedMtlNames))
            {
                yield return new MissingMaterialValidationHint(mtlName);
            }
        }

        if (flags.HasFlag(Validation.HasAllTextures))
        {
            foreach (var textureName in textureNames.Except(listedTextureNames))
            {
                yield return new MissingTextureValidationHint(textureName);
            }
        }

        var allParts = luminaire.Parts.SelectMany(GetParts).ToArray();

        foreach (var part in allParts)
        {
            var validationHints = part switch
            {
                LightEmittingSurfacePart lightEmittingSurfacePart => ValidateLightEmittingSurfacePart(lightEmittingSurfacePart,
                    flags,
                    allParts.OfType<LightEmittingPart>().ToArray(),
                    allParts.OfType<GeometryPart>().First(d => d.LightEmittingSurfaces?.Contains(lightEmittingSurfacePart) ?? false).GeometryReference.Model),
                JointPart jointPart => ValidateJointPart(jointPart, flags),
                LightEmittingPart lightEmittingPart => ValidateLightEmittingPart(lightEmittingPart, flags),
                GeometryPart geometryPart => ValidateGeometryPart(geometryPart, flags),
                SensorPart sensorPart => ValidateSensorPart(sensorPart, flags),
                _ => throw new ArgumentOutOfRangeException(nameof(part))
            };

            foreach (var validationHint in validationHints)
            {
                yield return validationHint;
            }
        }

        if (flags.HasFlag(Validation.NameConvention))
        {
            foreach (var duplicatedName in allParts.Select(x => x.Name).GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key))
            {
                yield return new L3DContentValidationHint(
                    $"{nameof(Part.Name)} of {nameof(Part)} '{duplicatedName}' has be unique");
            }
        }

        if (flags.HasFlag(Validation.MandatoryField))
        {
            if (string.IsNullOrWhiteSpace(luminaire.Header.CreatedWithApplication))
                yield return new L3DContentValidationHint(
                    $"{nameof(Header.CreatedWithApplication)} of {nameof(Header)} must not be null or whitespace");

            if (!luminaire.GeometryDefinitions.Any())
                yield return new L3DContentValidationHint(
                    $"{nameof(Luminaire.GeometryDefinitions)} of {nameof(Luminaire)} must not be empty");

            if (!luminaire.Parts.Any())
                yield return new L3DContentValidationHint(
                    $"{nameof(Luminaire.Parts)} of {nameof(Luminaire)} must not be empty");
        }

        if (flags.HasFlag(Validation.HasLightEmittingPart) && !allParts.OfType<LightEmittingPart>().Any())
            yield return new L3DContentValidationHint(
                $"{nameof(Luminaire.Parts)} of {nameof(Luminaire)} must not be empty");
    }

    private static bool TryValidatePartName(Part part, Validation flags, out ValidationHint? validationHint)
    {
        if (!flags.HasFlag(Validation.NameConvention))
        {
            validationHint = null;
            return false;
        }
        if (string.IsNullOrWhiteSpace(part.Name))
        {
            validationHint = new L3DContentValidationHint(
                $"{nameof(Part.Name)} of {part.GetType().Name} has be not empty or whitespace");
            return true;
        }
        if (!Regex.IsMatch(part.Name, @"^[A-Za-z][\w\.\-]{2,}$"))
        {
            validationHint = new L3DContentValidationHint(
                $"{nameof(Part.Name)} of {part.GetType().Name} has to match the pattern \"^[A-Za-z][\\w\\.\\-]{{2,}}$\"");
            return true;
        }

        validationHint = null;
        return false;
    }

    private static IEnumerable<ValidationHint> ValidateLightEmittingSurfacePart(LightEmittingSurfacePart lightEmittingSurfacePart, Validation flags, LightEmittingPart[] leos, IModel3D? model)
    {
        if (TryValidatePartName(lightEmittingSurfacePart, flags, out var nameHint))
            yield return nameHint!;

        foreach (var faceAssignment in lightEmittingSurfacePart.FaceAssignments)
        {
            if (flags.HasFlag(Validation.MinMaxRestriction) && faceAssignment.GroupIndex < 0)
                yield return new L3DContentValidationHint(
                    $"{nameof(FaceAssignment.GroupIndex)} of {nameof(FaceAssignment)} '{lightEmittingSurfacePart.Name}' must be greater equals 0");

            switch (faceAssignment)
            {
                case SingleFaceAssignment singleFaceAssignment:
                    {
                        if (flags.HasFlag(Validation.MinMaxRestriction) && singleFaceAssignment.FaceIndex < 0)
                            yield return new L3DContentValidationHint(
                                $"{nameof(SingleFaceAssignment.FaceIndex)} of {nameof(SingleFaceAssignment)} '{lightEmittingSurfacePart.Name}' must be greater equals 0");

                        if (flags.HasFlag(Validation.FaceReferences) && !(model?.IsFaceIndexValid(singleFaceAssignment.GroupIndex, singleFaceAssignment.FaceIndex) ?? false))
                            yield return new L3DContentValidationHint(
                                $"{nameof(SingleFaceAssignment.GroupIndex)}, {nameof(SingleFaceAssignment.FaceIndex)} of {nameof(SingleFaceAssignment)} '{lightEmittingSurfacePart.Name}' must be defined within the parent model definition");
                        break;
                    }
                case FaceRangeAssignment faceRangeAssignment:
                    {
                        if (flags.HasFlag(Validation.MinMaxRestriction) && faceRangeAssignment.FaceIndexBegin < 0)
                            yield return new L3DContentValidationHint(
                                $"{nameof(FaceRangeAssignment.FaceIndexBegin)} of {nameof(FaceRangeAssignment)} '{lightEmittingSurfacePart.Name}' must be greater equals 0");

                        if (flags.HasFlag(Validation.MinMaxRestriction) && faceRangeAssignment.FaceIndexEnd <= faceRangeAssignment.FaceIndexBegin)
                            yield return new L3DContentValidationHint(
                                $"{nameof(FaceRangeAssignment.FaceIndexEnd)} of {nameof(FaceRangeAssignment)} '{lightEmittingSurfacePart.Name}' must be greater than {nameof(FaceRangeAssignment.FaceIndexBegin)}");

                        for (var faceIndex = faceRangeAssignment.FaceIndexBegin; faceIndex <= faceRangeAssignment.FaceIndexEnd; faceIndex++)
                        {
                            if (flags.HasFlag(Validation.FaceReferences) && !(model?.IsFaceIndexValid(faceRangeAssignment.GroupIndex, faceIndex) ?? false))
                                yield return new L3DContentValidationHint(
                                    $"{nameof(FaceRangeAssignment.GroupIndex)}, {nameof(faceIndex)} of {nameof(FaceRangeAssignment)} '{lightEmittingSurfacePart.Name}' must be defined within the parent model definition");
                        }

                        break;
                    }
            }
        }

        foreach (var intensityMapping in lightEmittingSurfacePart.LightEmittingPartIntensityMapping)
        {
            if (flags.HasFlag(Validation.NameReferences) && string.IsNullOrWhiteSpace(intensityMapping.Key))
                yield return new L3DContentValidationHint(
                    $"{nameof(LightEmittingSurfacePart.LightEmittingPartIntensityMapping)}.[{intensityMapping.Key}] of {nameof(LightEmittingSurfacePart)} '{lightEmittingSurfacePart.Name}' must not be null or whitespace");

            if (flags.HasFlag(Validation.NameReferences) && leos.Any(d => !string.Equals(d.Name, intensityMapping.Key, StringComparison.Ordinal)))
                yield return new L3DContentValidationHint(
                    $"{nameof(LightEmittingSurfacePart.LightEmittingPartIntensityMapping)}.[{intensityMapping.Key}] of {nameof(LightEmittingSurfacePart)} '{lightEmittingSurfacePart.Name}' must be defined in any {nameof(LightEmittingPart)}.{nameof(LightEmittingPart.Name)}");

            if (flags.HasFlag(Validation.MinMaxRestriction) && intensityMapping.Value is < 0 or > 1)
                yield return new L3DContentValidationHint(
                    $"{nameof(LightEmittingSurfacePart.LightEmittingPartIntensityMapping)}.[{intensityMapping.Key}] of {nameof(LightEmittingSurfacePart)} '{lightEmittingSurfacePart.Name}' must be between 0 and 1 but was '{intensityMapping.Value}'");
        }
    }

    private static IEnumerable<ValidationHint> ValidateJointPart(JointPart jointPart, Validation flags)
    {
        if (TryValidatePartName(jointPart, flags, out var nameHint))
            yield return nameHint!;

        if (flags.HasFlag(Validation.MinMaxRestriction) && jointPart.XAxis != null)
        {
            if (jointPart.XAxis.Max <= jointPart.XAxis.Min)
                yield return new L3DContentValidationHint(
                    $"{nameof(AxisRotation.Max)} of {nameof(JointPart.XAxis)} '{jointPart.Name}' must be greater than {nameof(AxisRotation.Min)}");

            if (jointPart.XAxis.Step <= 0)
                yield return new L3DContentValidationHint(
                    $"{nameof(AxisRotation.Step)} of {nameof(JointPart.XAxis)} '{jointPart.Name}' must be greater than 0");
        }

        if (flags.HasFlag(Validation.MinMaxRestriction) && jointPart.YAxis != null)
        {
            if (jointPart.YAxis.Max <= jointPart.YAxis.Min)
                yield return new L3DContentValidationHint(
                    $"{nameof(AxisRotation.Max)} of {nameof(JointPart.YAxis)} '{jointPart.Name}' must be greater than {nameof(AxisRotation.Min)}");

            if (jointPart.YAxis.Step <= 0)
                yield return new L3DContentValidationHint(
                    $"{nameof(AxisRotation.Step)} of {nameof(JointPart.YAxis)} '{jointPart.Name}' must be greater than 0");
        }

        if (flags.HasFlag(Validation.MinMaxRestriction) && jointPart.ZAxis != null)
        {
            if (jointPart.ZAxis.Max <= jointPart.ZAxis.Min)
                yield return new L3DContentValidationHint(
                    $"{nameof(AxisRotation.Max)} of {nameof(JointPart.ZAxis)} '{jointPart.Name}' must be greater than {nameof(AxisRotation.Min)}");

            if (jointPart.ZAxis.Step <= 0)
                yield return new L3DContentValidationHint(
                    $"{nameof(AxisRotation.Step)} of {nameof(JointPart.ZAxis)} '{jointPart.Name}' must be greater than 0");
        }

        if (flags.HasFlag(Validation.MandatoryField) && !jointPart.Geometries.Any())
            yield return new L3DContentValidationHint(
                $"{nameof(JointPart.Geometries)} of {nameof(JointPart)} '{jointPart.Name}' must not be empty");
    }

    private static IEnumerable<ValidationHint> ValidateLightEmittingPart(LightEmittingPart lightEmittingPart, Validation flags)
    {
        if (TryValidatePartName(lightEmittingPart, flags, out var nameHint))
            yield return nameHint!;

        if (!flags.HasFlag(Validation.MinMaxRestriction))
            yield break;

        switch (lightEmittingPart.Shape)
        {
            case Circle circle:
                if (circle.Diameter <= 0)
                    yield return new L3DContentValidationHint(
                        $"{nameof(Circle.Diameter)} of {nameof(LightEmittingPart.Shape)} '{lightEmittingPart.Name}' must be greater than 0");
                break;
            case Rectangle rectangle:
                if (rectangle.SizeX <= 0)
                    yield return new L3DContentValidationHint(
                        $"{nameof(Rectangle.SizeX)} of {nameof(LightEmittingPart.Shape)} '{lightEmittingPart.Name}' must be greater than 0");
                if (rectangle.SizeY <= 0)
                    yield return new L3DContentValidationHint(
                        $"{nameof(Rectangle.SizeY)} of {nameof(LightEmittingPart.Shape)} '{lightEmittingPart.Name}' must be greater than 0");
                break;
        }
    }

    private static IEnumerable<ValidationHint> ValidateGeometryPart(GeometryPart geometryPart, Validation flags)
    {
        if (TryValidatePartName(geometryPart, flags, out var nameHint))
            yield return nameHint!;

        if (!flags.HasFlag(Validation.GeometryReferences))
            yield break;

        if (string.IsNullOrWhiteSpace(geometryPart.GeometryReference.FileName))
            yield return new L3DContentValidationHint(
                $"{nameof(GeometryPart.GeometryReference.FileName)} of {nameof(GeometryPart.GeometryReference)} '{geometryPart.Name}' must be not null or whitespace");

        if (string.IsNullOrWhiteSpace(geometryPart.GeometryReference.GeometryId))
        {
            yield return new L3DContentValidationHint(
                $"{nameof(GeometryPart.GeometryReference.GeometryId)} of {nameof(GeometryPart.GeometryReference)} '{geometryPart.Name}' must be not null or whitespace");
            yield break;
        }

        if (geometryPart.GeometryReference.Model == null)
        {
            yield return new L3DContentValidationHint(
                $"{nameof(GeometryPart.GeometryReference.Model)} of {nameof(GeometryPart.GeometryReference)} '{geometryPart.Name}' must be not null");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(geometryPart.GeometryReference.Model.FileName))
            yield return new L3DContentValidationHint(
                $"{nameof(GeometryPart.GeometryReference.Model.FileName)} of {nameof(GeometryPart.GeometryReference.Model)} '{geometryPart.Name}' must be not null or whitespace");

        if (geometryPart.GeometryReference.Model.Data == null)
            yield return new L3DContentValidationHint(
                $"{nameof(GeometryPart.GeometryReference.Model.Data)} of {nameof(GeometryPart.GeometryReference.Model)} '{geometryPart.Name}' must be not null");
    }

    private static IEnumerable<ValidationHint> ValidateSensorPart(SensorPart sensorPart, Validation flags)
    {
        if (TryValidatePartName(sensorPart, flags, out var nameHint))
            yield return nameHint!;
    }

    private static IEnumerable<GeometryPart> GetGeometryParts(GeometryPart geometryPart)
    {
        yield return geometryPart;

        foreach (var subGeometryPart in geometryPart.Joints?.SelectMany(x => x.Geometries) ?? Array.Empty<GeometryPart>())
        {
            foreach (var part in GetGeometryParts(subGeometryPart))
            {
                yield return part;
            }
        }
    }

    private static IEnumerable<Part> GetParts(Part part)
    {
        yield return part;

        var parts = part switch
        {
            GeometryPart geometryPart => geometryPart.LightEmittingObjects?
                .Union<Part>(geometryPart.LightEmittingSurfaces ?? new())
                .Union(geometryPart.Joints ?? new())
                .Union(geometryPart.Sensors ?? new()) ?? Array.Empty<Part>(),
            JointPart jointPart => jointPart.Geometries,
            _ => Array.Empty<Part>()
        };

        foreach (var innerPart in parts)
        {
            foreach (var nextPart in GetParts(innerPart))
            {
                yield return nextPart;
            }
        }
    }
}