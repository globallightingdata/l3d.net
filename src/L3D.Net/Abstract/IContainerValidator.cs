using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Abstract;

internal interface IContainerValidator
{
    IEnumerable<ValidationHint> Validate(string containerPath, Validation flags);

    IEnumerable<ValidationHint> Validate(byte[] containerBytes, Validation flags);

    IEnumerable<ValidationHint> Validate(Stream containerStream, Validation flags);
}