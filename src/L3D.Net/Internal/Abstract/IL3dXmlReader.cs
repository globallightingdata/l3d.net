using L3D.Net.Data;

namespace L3D.Net.Internal.Abstract
{
    internal interface IL3dXmlReader
    {
        Luminaire Read(string filename, string workingDirectory);
    }
}