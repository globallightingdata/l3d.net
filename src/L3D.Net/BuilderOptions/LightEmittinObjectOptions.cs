using System;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net.BuilderOptions
{
    public class LightEmittinObjectOptions : TransformableOptions
    {
        private readonly ILightEmittingSurfaceHolder _lesHolder;

        internal new LightEmittingPart Data => (LightEmittingPart) base.Data;

        internal LightEmittinObjectOptions(ILuminaireBuilder builder, LightEmittingPart lightEmittingPart, ILightEmittingSurfaceHolder parent, ILogger logger)
            : base(builder, lightEmittingPart, logger)
        {
            _lesHolder = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public LightEmittinObjectOptions WithLuminousHeights(double c0, double c90, double c180, double c270)
        {
            Data.LuminousHeights = new LuminousHeights(c0, c90, c180, c270);
            return this;
        }

        public LightEmittinObjectOptions WithLightEmittingSurfacesOnParent(int faceIndexBegin, int faceIndexEnd, int groupIndex = 0)
        {
            _lesHolder.CreateLightEmittingSurfaces(Data.Name, faceIndexBegin, faceIndexEnd, groupIndex);
            return this;
        }

        public LightEmittinObjectOptions WithLightEmittingSurfaceOnParent(int faceIndex, int groupIndex = 0)
        {
            _lesHolder.CreateLightEmittingSurface(Data.Name, faceIndex, groupIndex);
            return this;
        }

    }
}