using System;
using System.Numerics;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net.BuilderOptions
{
    public class GeometryOptions : TransformableOptions, ILightEmittingSurfaceHolder
    {
        internal GeometryOptions(ILuminaireBuilder builder, GeometryPart geometry, ILogger logger)
            : base(builder, geometry, logger)
        {
        }

        internal new GeometryPart Data => (GeometryPart)base.Data;

        public GeometryOptions AddJoint(string partName, Func<JointOptions, JointOptions> options = null)
        {
            LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);

            var jointNode = new JointPart(partName);
            Data.Joints.Add(jointNode);

            options?.Invoke(new JointOptions(LuminaireBuilder, jointNode, Logger));

            return this;
        }

        public GeometryOptions AddRectangularLightEmittingObject(string partName, double sizeX, double sizeY,
            Func<LightEmittinObjectOptions, LightEmittinObjectOptions> options = null)
        {
            LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);

            var shape = new Rectangle(sizeX, sizeY);
            var leo = new LightEmittingPart(partName, shape);
            Data.LightEmittingObjects.Add(leo);

            options?.Invoke(new LightEmittinObjectOptions(LuminaireBuilder, leo, this, Logger));

            return this;
        }

        public GeometryOptions AddCircularLightEmittingObject(string partName, double diameter,
            Func<LightEmittinObjectOptions, LightEmittinObjectOptions> options = null)
        {
            LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);

            var shape = new Circle(diameter);
            var leo = new LightEmittingPart(partName, shape);
            Data.LightEmittingObjects.Add(leo);

            options?.Invoke(new LightEmittinObjectOptions(LuminaireBuilder, leo, this, Logger));

            return this;
        }

        public GeometryOptions AddSensorObject(string partName, Func<SensorOptions, SensorOptions> options = null)
        {
            LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);

            var sensor = new SensorPart(partName);
            Data.Sensors.Add(sensor);

            options?.Invoke(new SensorOptions(LuminaireBuilder, sensor, Logger));

            return this;
        }

        public GeometryOptions WithLightEmittingSurface(string partName,
            Func<LightEmittingSurfaceOptions, LightEmittingSurfaceOptions> options)
        {
            LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);

            var les = new LightEmittingSurfacePart(partName);
            Data.LightEmittingSurfaces.Add(les);

            options.Invoke(new LightEmittingSurfaceOptions(LuminaireBuilder, les, Data, Logger));

            return this;
        }

        public GeometryOptions WithExcludedFromMeasurement(bool excluded = true)
        {
            Data.ExcludedFromMeasurement = excluded;
            return this;
        }

        public GeometryOptions WithElectricalConnector(Vector3 position)
        {
            return WithElectricalConnector(position.X, position.Y, position.Z);
        }

        public GeometryOptions WithElectricalConnector(double x, double y, double z)
        {
            return WithElectricalConnector((float)x, (float)y, (float)z);
        }

        public GeometryOptions WithElectricalConnector(float x, float y, float z)
        {
            Data.ElectricalConnectors.Add(new Vector3
            {
                X = x,
                Y = y,
                Z = z
            });
            return this;
        }

        public GeometryOptions WithPendulumConnector(Vector3 position)
        {
            return WithPendulumConnector(position.X, position.Y, position.Z);
        }

        public GeometryOptions WithPendulumConnector(double x, double y, double z)
        {
            return WithPendulumConnector((float)x, (float)y, (float)z);
        }

        public GeometryOptions WithPendulumConnector(float x, float y, float z)
        {
            Data.PendulumConnectors.Add(new Vector3
            {
                X = x,
                Y = y,
                Z = z
            });
            return this;
        }

        void ILightEmittingSurfaceHolder.WithLightEmittingSurface(string lesPartName,
            Action<ILightEmittingSurfaceOptions> options)
        {
            WithLightEmittingSurface(lesPartName, lesOptions =>
            {
                options(lesOptions);
                return lesOptions;
            });
        }
    }
}