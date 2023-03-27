using L3D.Net.Geometry;
using L3D.Net.Internal.Abstract;

namespace L3D.Net.Data;

public class GeometrySource
{
    public string GeometryId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public IModel3D Model { get; set; } = new ObjModel3D();
    public GeometricUnits Units { get; set; }
}