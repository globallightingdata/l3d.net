using L3D.Net.XML;

namespace L3D.Net.Data;

public class FormatVersion
{
    public static readonly FormatVersion CurrentVersion = new()
    {
        Major = GlobalXmlDefinitions.CurrentVersion.Major,
        Minor = GlobalXmlDefinitions.CurrentVersion.Minor
    };

    private int _preRelease;

    public int Major { get; set; }

    public int Minor { get; set; }

    public int PreRelease
    {
        get => _preRelease;
        set
        {
            _preRelease = value;
            PreReleaseSpecified = true;
        }
    }

    public bool PreReleaseSpecified { get; set; }

    public override string ToString() => $"v{Major}.{Minor}" + (PreReleaseSpecified ? $"-rc{PreRelease}" : string.Empty);
}