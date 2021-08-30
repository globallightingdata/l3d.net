namespace L3D.Net.API.Dto
{
    public class LightEmittingPartDto : PartDto
    {
        public ShapeDto Shape { get; set; }

        public LuminousHeightsDto LuminousHeights { get; set; }
    }
}