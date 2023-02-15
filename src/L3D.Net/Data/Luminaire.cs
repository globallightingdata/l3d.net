using System.Collections.Generic;

namespace L3D.Net.Data;

internal class Luminaire
{
    public Header Header { get; } = new Header();

    public List<GeometryDefinition> GeometryDefinitions { get; } = new List<GeometryDefinition>();

    public List<GeometryPart> Parts { get; } = new List<GeometryPart>();
}