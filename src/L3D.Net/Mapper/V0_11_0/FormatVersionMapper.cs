using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;

namespace L3D.Net.Mapper.V0_11_0
{
    public class FormatVersionMapper : DtoMapperBase<FormatVersion, FormatVersionDto>
    {
        public static readonly FormatVersionMapper Instance = new();

        protected override FormatVersion ConvertData(FormatVersionDto element) => new()
        {
            Major = element.Major,
            Minor = element.Minor,
            PreRelease = element.PreRelease,
            PreReleaseSpecified = element.PreReleaseSpecified
        };

        protected override FormatVersionDto ConvertData(FormatVersion element) => new()
        {
            Major = element.Major,
            Minor = element.Minor,
            PreRelease = element.PreRelease,
            PreReleaseSpecified = element.PreReleaseSpecified
        };
    }
}
