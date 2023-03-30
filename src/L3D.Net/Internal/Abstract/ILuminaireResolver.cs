using L3D.Net.Data;
using Microsoft.Extensions.Logging;

namespace L3D.Net.Internal.Abstract
{
    internal interface ILuminaireResolver
    {
        Luminaire Resolve(Luminaire luminaire, ContainerCache cache, ILogger? logger = null);
    }
}