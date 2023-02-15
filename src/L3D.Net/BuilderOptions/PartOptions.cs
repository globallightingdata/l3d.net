using System;
using L3D.Net.Data;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;

namespace L3D.Net.BuilderOptions;

public class PartOptions
{
    internal PartOptions(ILuminaireBuilder builder, Part data, ILogger logger)
    {
        LuminaireBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Logger = logger;
    }

    internal ILuminaireBuilder LuminaireBuilder { get; }
    internal Part Data { get; }
    internal ILogger Logger { get; }
}