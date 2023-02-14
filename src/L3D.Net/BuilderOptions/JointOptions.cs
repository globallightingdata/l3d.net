using System;
using System.Numerics;
using L3D.Net.API.Dto;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net.BuilderOptions;

public class JointOptions : TransformableOptions
{
    internal JointOptions(ILuminaireBuilder builder, JointPart jointPart, ILogger logger)
        : base(builder, jointPart, logger)
    {
    }

    internal new JointPart Data => (JointPart) base.Data;

    public JointOptions WithXAxisDegreesOfFreedom(double min, double max, double step)
    {
        Data.XAxis = new AxisRotation(min, max, step);
        return this;
    }

    public JointOptions WithYAxisDegreesOfFreedom(double min, double max, double step)
    {
        Data.YAxis = new AxisRotation(min, max, step);
        return this;
    }

    public JointOptions WithZAxisDegreesOfFreedom(double min, double max, double step)
    {
        Data.ZAxis = new AxisRotation(min, max, step);
        return this;
    }

    public JointOptions AddGeometry(string partName, string modelPath, GeometricUnits units, Func<GeometryOptions, GeometryOptions> options = null)
    {
        LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);

        var definitionDto = LuminaireBuilder.EnsureGeometryFileDefinition(modelPath, units);
        var geometryNode = new GeometryPart(partName, definitionDto);
        Data.Geometries.Add(geometryNode);

        options?.Invoke(new GeometryOptions(LuminaireBuilder, geometryNode, Logger));

        return this;
    }

    public JointOptions WithDefaultRotation(float x, float y, float z)
    {
        return WithDefaultRotation(new Vector3
        {
            X = x,
            Y = y,
            Z = z
        });
    }

    public JointOptions WithDefaultRotation(Vector3 rotation)
    {
        Data.DefaultRotation = rotation;

        return this;
    }

    public JointOptions WithDefaultRotation(double x, double y, double z)
    {
        return WithDefaultRotation((float) x, (float) y, (float) z);
    }
}