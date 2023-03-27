using L3D.Net.Data;
using L3D.Net.XML.V0_9_2.Dto;

namespace L3D.Net.Mapper.V0_9_2
{
    public class LuminousHeightsMapper : DtoMapperBase<LuminousHeights, LuminousHeightsDto>
    {
        public static readonly LuminousHeightsMapper Instance = new();

        protected override LuminousHeights ConvertData(LuminousHeightsDto element) => new()
        {
            C0 = element.C0,
            C90 = element.C90,
            C180 = element.C180,
            C270 = element.C270
        };

        protected override LuminousHeightsDto ConvertData(LuminousHeights element) => new()
        {
            C0 = element.C0,
            C90 = element.C90,
            C180 = element.C180,
            C270 = element.C270
        };
    }
}
