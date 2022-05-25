using System;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net.BuilderOptions
{
    public class LightEmittingSurfaceOptions : PartOptions, ILightEmittingSurfaceOptions
    {
        private readonly GeometryPart _geometryPart;

        internal LightEmittingSurfaceOptions(ILuminaireBuilder builder, LightEmittingSurfacePart les,
            GeometryPart geometryPart, ILogger logger) :
            base(builder, les, logger)
        {
            _geometryPart = geometryPart ?? throw new ArgumentNullException(nameof(geometryPart));
        }

        internal new LightEmittingSurfacePart Data => (LightEmittingSurfacePart)base.Data;

        public LightEmittingSurfaceOptions WithLightEmittingPart(string leoPartName, double intensity = 1.0)
        {
            if (!LuminaireBuilder.IsValidLightEmittingPartName(leoPartName))
                throw new ArgumentException(
                    "The given light emitting part name is not known. Please declare the light emitting part first!");

            Data.AddLightEmittingObject(leoPartName, intensity);

            return this;
        }

        public LightEmittingSurfaceOptions WithSurfaceRange(int faceIndexBegin, int faceIndexEnd, int groupIndex = 0)
        {
            if (faceIndexBegin == faceIndexEnd)
                return WithSurface(faceIndexBegin, groupIndex);

            if (!_geometryPart.GeometryDefinition.Model.IsFaceIndexValid(groupIndex, faceIndexBegin))
                Logger?.Log(LogLevel.Warning,
                    $@"The given groupIndex({groupIndex})/faceIndexBegin({faceIndexBegin}) combination is not valid!");

            if (!_geometryPart.GeometryDefinition.Model.IsFaceIndexValid(groupIndex, faceIndexEnd))
                Logger?.Log(LogLevel.Warning,
                    $@"The given groupIndex({groupIndex})/faceIndexEnd({faceIndexEnd}) combination is not valid!");

            Data.AddFaceAssignment(groupIndex, faceIndexBegin, faceIndexEnd);

            return this;
        }

        public LightEmittingSurfaceOptions WithSurface(int faceIndex, int groupIndex = 0)
        {
            if (!_geometryPart.GeometryDefinition.Model.IsFaceIndexValid(groupIndex, faceIndex))
                Logger?.Log(LogLevel.Warning,
                    $@"The given groupIndex({groupIndex})/faceIndex({faceIndex}) combination is not valid!");

            Data.AddFaceAssignment(groupIndex, faceIndex);
            return this;
        }

        ILightEmittingSurfaceOptions ILightEmittingSurfaceOptions.WithLightEmittingPart(string partName)
        {
            return WithLightEmittingPart(partName);
        }

        ILightEmittingSurfaceOptions ILightEmittingSurfaceOptions.WithSurface(int faceIndex, int groupIndex)
        {
            return WithSurface(faceIndex, groupIndex);
        }

        ILightEmittingSurfaceOptions ILightEmittingSurfaceOptions.WithSurfaceRange(int faceIndexBegin, int faceIndexEnd,
            int groupIndex)
        {
            return WithSurfaceRange(faceIndexBegin, faceIndexEnd, groupIndex);
        }
    }
}