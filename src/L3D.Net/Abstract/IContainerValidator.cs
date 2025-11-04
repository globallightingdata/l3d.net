using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Abstract;

internal interface IContainerValidator
{
    IEnumerable<ValidationHint> Validate(string containerPath, Validation flags);

    IEnumerable<ValidationHint> Validate(byte[] containerBytes, Validation flags);

    IEnumerable<ValidationHint> Validate(Stream containerStream, Validation flags);

    ValidationResultContainer CreateValidationResult(string containerPath, Validation flags);

    ValidationResultContainer CreateValidationResult(byte[] containerBytes, Validation flags);

    ValidationResultContainer CreateValidationResult(Stream containerStream, Validation flags);
}