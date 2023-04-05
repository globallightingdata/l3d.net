using L3D.Net.Data;
using L3D.Net.XML.V0_10_0.Dto;
using System.Linq;

namespace L3D.Net.Mapper.V0_11_0
{
    public class LuminaireMapper : DtoMapperBase<Luminaire, LuminaireDto>
    {
        public static readonly LuminaireMapper Instance = new();

        protected override Luminaire ConvertData(LuminaireDto element) => new()
        {
            Header = HeaderMapper.Instance.Convert(element.Header),
            GeometryDefinitions = element.GeometryDefinitions.Select(x => GeometryDefinitionMapper.Instance.Convert(x)).ToList(),
            Parts = element.Parts.Select(PartMapper.Instance.Convert).Where(x => x != null).Cast<GeometryPart>().ToList()
        };

        protected override LuminaireDto ConvertData(Luminaire element) => new()
        {
            Header = HeaderMapper.Instance.Convert(element.Header),
            GeometryDefinitions = element.GeometryDefinitions.Select(x => GeometryDefinitionMapper.Instance.Convert(x)).ToList(),
            Parts = element.Parts.Select(PartMapper.Instance.Convert).Where(x => x != null).Cast<GeometryPartDto>().ToList()
        };
    }
}
