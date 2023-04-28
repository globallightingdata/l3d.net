using System.Xml.Serialization;

namespace L3D.Net.XML.V0_11_0.Dto
{
    public class FormatVersionDto
    {
        private int _preRelease;

        [XmlAttribute(AttributeName = "major")]
        public int Major { get; set; }

        [XmlAttribute(AttributeName = "minor")]
        public int Minor { get; set; }

        [XmlAttribute(AttributeName = "pre-release")]
        public int PreRelease
        {
            get => _preRelease;
            set
            {
                _preRelease = value;
                PreReleaseSpecified = true;
            }
        }

        [XmlIgnore]
        public bool PreReleaseSpecified { get; set; }
        public override string ToString() => $"v{Major}.{Minor}" + (PreReleaseSpecified ? $"-rc{PreRelease}" : string.Empty);
    }
}
