using L3D.Net.Internal.Abstract;

namespace L3D.Net.Data;

public class GeometryFileDefinition : GeometryReference
{
    public string FileName { get; set; } = string.Empty;
    public IModel3D? Model { get; set; }
    public GeometricUnits Units { get; set; }
}