using System;

namespace L3D.Net.XML;

public static class GlobalXmlDefinitions
{
    public static readonly Version CurrentVersion = new(0, 11, 0, 0);

    public static Version GetNextMatchingVersion(Version other)
    {
        if (other.Major <= CurrentVersion.Major)
            return CurrentVersion;
        return other;
    }
}