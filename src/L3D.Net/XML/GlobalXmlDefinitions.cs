using System;
using System.Text.RegularExpressions;

namespace L3D.Net.XML;

public static class GlobalXmlDefinitions
{
    public static readonly Regex VersionRegex = new(@"https://gldf.io/xsd/l3d/(\d{1,3}\.\d{1,3}(?:\.\d{1,6})?)/l3d.xsd",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    public static readonly Version CurrentVersion = new(0, 11, 0, 0);

    public static bool IsParseable(Version other) =>
        other.Major <= CurrentVersion.Major;

    public static Version GetNextMatchingVersion(Version other)
    {
        if (other.Major <= CurrentVersion.Major)
            return CurrentVersion;
        return other;
    }
}