using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using L3D.Net.API.Dto;
using L3D.Net.BuilderOptions;
using L3D.Net.Data;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net;

public class LuminaireBuilder : ILuminaireBuilder
{
    private readonly IObjParser _objParser;
    private readonly IContainerBuilder _containerBuilder;
    private readonly ILogger _logger;
    private int _nextGeomDefinitionId = 1;
        
    internal LuminaireBuilder(string toolNameAndVersion, IObjParser objParser, IContainerBuilder containerBuilder, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(toolNameAndVersion))
            throw new ArgumentException(@"Please pass some Tooling information!");

        _objParser = objParser ?? throw new ArgumentNullException(nameof(objParser));
        _containerBuilder = containerBuilder ?? throw new ArgumentNullException(nameof(containerBuilder));
        _logger = logger;

        Luminaire = new Luminaire
        {
            Header = {CreatedWithApplication = toolNameAndVersion}
        };
    }

    internal Luminaire Luminaire { get; }

    public LuminaireBuilder WithTool(string toolNameAndVersion)
    {
        if (string.IsNullOrWhiteSpace(toolNameAndVersion))
            throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(toolNameAndVersion));

        Luminaire.Header.CreatedWithApplication = toolNameAndVersion;
        return this;
    }

    public LuminaireBuilder WithModelName(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            modelName = null;

        Luminaire.Header.Name = modelName;

        return this;
    }

    public LuminaireBuilder WithDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            description = null;

        Luminaire.Header.Description = description;

        return this;
    }

    public LuminaireBuilder AddGeometry(string partName, string modelFilePath, GeometricUnits modelUnits, Func<GeometryOptions, GeometryOptions> options = null)
    {
        ThrowWhenPartNameIsInvalid(partName);

        var geomDefinition = EnsureGeometryFileDefinition(modelFilePath, modelUnits);
        var geometryNode = new GeometryPart(partName, geomDefinition);
        Luminaire.Parts.Add(geometryNode);

        options?.Invoke(new GeometryOptions(this, geometryNode, _logger));
            
        return this;
    }

    public void Build(string containerPath)
    {
        _containerBuilder.CreateContainer(Luminaire, containerPath);
    }

    #region Internal

    internal LuminaireBuilder WithCreatedTime(DateTime creationTime)
    {
        Luminaire.Header.CreationTimeCode = creationTime;

        return this;
    }

    internal GeometryDefinition EnsureGeometryFileDefinition(string modelFilePath, GeometricUnits modelUnits)
    {
        if (string.IsNullOrWhiteSpace(modelFilePath))
            throw new ArgumentException(@"The model file name must not be null or empty!", nameof(modelFilePath));

        var absoluteModelFilePath = Path.GetFullPath(modelFilePath);

        var fileDefinition = Luminaire.GeometryDefinitions.FirstOrDefault(definition => definition.Model.FilePath == absoluteModelFilePath);
        if (fileDefinition != null)
        {
            if (fileDefinition.Units != modelUnits)
                throw new ArgumentException(@"All geometries for the same model file must have the same units!", nameof(modelUnits));
        }
        else
        {
            if (!File.Exists(absoluteModelFilePath))
                throw new FileNotFoundException(@"The given model file is not available!", absoluteModelFilePath);

            fileDefinition = CreateGeometryDefinition(absoluteModelFilePath, modelUnits);
                
            Luminaire.GeometryDefinitions.Add(fileDefinition);
        }

        return fileDefinition;
    }
        
    internal bool IsValidLightEmittingPartName(string leoPartName)
    {
        if (string.IsNullOrWhiteSpace(leoPartName))
            return false;

        return TryGetPartByName(leoPartName, out var part) && part is LightEmittingPart;
    }

    internal bool TryGetPartByName(string partName, out Part part)
    {
        part = null;
            
        if (string.IsNullOrWhiteSpace(partName))
            return false;

        for (var i = 0; i < Luminaire.Parts.Count && part == null; i++)
        {
            part = GetPartByName(partName, Luminaire.Parts[i]);
        }

        return part != null;
    }

    internal void ThrowWhenPartNameIsInvalid(string partName)
    {
        if (string.IsNullOrWhiteSpace(partName))
            throw new ArgumentException(@"The part name must not be null or empty!", nameof(partName));

        if (!Regex.IsMatch(partName, @"^[A-Za-z][\w\.\-]{2,}$"))
            throw new ArgumentException("partName has to match the pattern \"^[A-Za-z][\\w\\.\\-]{2,}$\"");

        if (TryGetPartByName(partName, out _))
            throw new ArgumentException($@"The model name {partName} is already availbale!", nameof(partName));
    }

    #endregion

    #region IBuilder

    GeometryDefinition ILuminaireBuilder.EnsureGeometryFileDefinition(string modelFilePath, GeometricUnits modelUnits) => EnsureGeometryFileDefinition(modelFilePath, modelUnits);

    void ILuminaireBuilder.ThrowWhenPartNameIsInvalid(string partName) => ThrowWhenPartNameIsInvalid(partName);

    bool ILuminaireBuilder.IsValidLightEmittingPartName(string leoPartName) => IsValidLightEmittingPartName(leoPartName);

    #endregion
        
    #region private

    private GeometryDefinition CreateGeometryDefinition(string modelPath, GeometricUnits modelUnits)
    {
        IModel3D model3D;
        try
        {
            model3D = _objParser.Parse(modelPath, _logger);
        }
        catch (Exception e)
        {
            throw new ModelParseException($"Failed to parse model file: {modelPath}", e.InnerException);
        }

        var id = $"geom_{_nextGeomDefinitionId++}";

        return new GeometryDefinition(id, model3D, modelUnits);
    }

    private Part GetPartByName(string partName, GeometryPart geometry)
    {
        if (geometry.Name == partName)
            return geometry;
            
        foreach (var joint in geometry.Joints)
        {
            var part = GetPartByName(partName, joint);
            if (part != null)
                return part;
        }

        foreach (var leo in geometry.LightEmittingObjects)
        {
            var part = GetPartByName(partName, leo);
            if (part != null)
                return part;
        }

        return null;
    }

    private Part GetPartByName(string partName, JointPart joint)
    {
        if (joint.Name == partName)
            return joint;

        foreach (var geom in joint.Geometries)
        {
            var part = GetPartByName(partName, geom);
            if (part != null)
                return part;
        }

        return null;
    }

    private Part GetPartByName(string partName, LightEmittingPart leo)
    {
        if (leo.Name == partName)
            return leo;

        return null;
    }

    #endregion
}