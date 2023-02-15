using L3D.Net.Data;

namespace L3D.Net.Internal.Abstract;

internal interface IContainerBuilder
{
    void CreateContainer(Luminaire luminaire, string containerPath);
}