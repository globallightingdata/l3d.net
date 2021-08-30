using System.Collections.Generic;

namespace L3D.Net.API.Dto
{
    public class LuminaireDto
    {
        public HeaderDto Header { get; set; }
        public IEnumerable<GeometryDefinitionDto> GeometryDefinitions { get; set; }
        public IEnumerable<GeometryPartDto> Parts { get; set; }
    }
}
