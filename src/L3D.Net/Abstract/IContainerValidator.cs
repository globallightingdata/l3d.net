using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Abstract;

internal interface IContainerValidator
{
    public IEnumerable<ValidationHint> Validate(string containerPath, Validation flags);

    public IEnumerable<ValidationHint> Validate(byte[] containerBytes, Validation flags);

    public IEnumerable<ValidationHint> Validate(Stream containerStream, Validation flags);

    public ValidationResultContainer CreateValidationResult(string containerPath, Validation flags);

    public ValidationResultContainer CreateValidationResult(byte[] containerBytes, Validation flags);

    public ValidationResultContainer CreateValidationResult(Stream containerStream, Validation flags);
}