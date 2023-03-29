using System.Collections.Generic;

namespace L3D.Net.Data;

public class Luminaire
{
    public Header? Header { get; set; }

    public List<GeometryFileDefinition> GeometryDefinitions { get; set; } = new();

    public List<GeometryPart> Parts { get; set; } = new();
}