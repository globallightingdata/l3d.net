namespace L3D.Net.Abstract;

internal static class Constants
{
#pragma warning disable S1075
    public const string CurrentSchemeUri = @"https://gldf.io/xsd/l3d/0.11.0/l3d.xsd";
#pragma warning restore S1075
    public const string L3DExtension = ".l3d";
    public const string L3DXmlFilename = "structure.xml";
    public const string L3DFormatVersionPath = "Luminaire/Header/FormatVersion";
    public const string L3DFormatVersionMajor = "major";
    public const string L3DFormatVersionMinor = "minor";
    public const string L3DFormatVersionPreRelease = "pre-release";
    public static readonly string[] L3DFormatVersionRequiredFields = [L3DFormatVersionMajor, L3DFormatVersionMinor];
}