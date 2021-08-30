using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net.BuilderOptions
{
    public class TransformableOptions : PartOptions
    {
        internal TransformableOptions(ILuminaireBuilder builder, TransformablePart data, ILogger logger) 
            : base(builder, data, logger)
        {
        }

        internal new TransformablePart Data => (TransformablePart) base.Data;
    }
}