using L3D.Net.API.Dto;
using L3D.Net.Data;

namespace L3D.Net.Internal.Abstract
{
    interface ILuminaireBuilder
    {
        void ThrowWhenPartNameIsInvalid(string partName);
        bool IsValidLightEmittingPartName(string leoPartName);
        GeometryDefinition EnsureGeometryFileDefinition(string modelPath, GeometricUnits units);
    }
}
