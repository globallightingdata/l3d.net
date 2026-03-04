using L3D.Net.Data;

namespace L3D.Net.Internal.Abstract;

internal interface IL3DXmlReader
{
    public Luminaire? Read(ContainerCache cache);
}