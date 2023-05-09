namespace L3D.Net.Abstract;

internal static class Constants
{
#pragma warning disable S1075
    public const string CurrentSchemeUri = @"https://gldf.io/xsd/l3d/0.11.0/l3d.xsd";
#pragma warning restore S1075
    public const string L3dExtension = ".l3d";
    public const string L3dXmlFilename = "structure.xml";
    public const string L3dFormatVersionPath = "Luminaire/Header/FormatVersion";
    public const string L3dFormatVersionMajor = "major";
    public const string L3dFormatVersionMinor = "minor";
    public const string L3dFormatVersionPreRelease = "pre-release";
    public static readonly string[] L3dFormatVersionRequiredFields = { L3dFormatVersionMajor, L3dFormatVersionMinor };
}