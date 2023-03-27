using L3D.Net.Data;

namespace L3D.Net.Internal.Abstract;

public interface IL3DXmlReader
{
    Luminaire Read(string filename, string workingDirectory);
}