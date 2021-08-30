using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net.BuilderOptions
{
    public class SensorOptions : TransformableOptions
    {
        internal SensorOptions(ILuminaireBuilder builder, SensorPart sensorPart, ILogger logger)
            : base(builder, sensorPart, logger)
        {
        }
    }
}