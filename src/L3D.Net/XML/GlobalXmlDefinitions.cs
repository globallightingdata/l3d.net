using System.Collections.Generic;

namespace L3D.Net.XML;

public static class GlobalXmlDefinitions
{
    public static readonly Dictionary<string, L3dXmlVersion> SchemeVersions = new()
    {
        { Constants.CurrentSchemeUri, L3dXmlVersion.V0_9_2 }
    };
}