using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Abstract;

public interface IValidator
{
    IEnumerable<ValidationHint> ValidateContainer(string containerPath, Validation flags);

    IEnumerable<ValidationHint> ValidateContainer(byte[] containerBytes, Validation flags);

    IEnumerable<ValidationHint> ValidateContainer(Stream containerStream, Validation flags);

    ValidationResultContainer CreateValidationResult(string containerPath, Validation flags);

    ValidationResultContainer CreateValidationResult(byte[] containerBytes, Validation flags);

    ValidationResultContainer CreateValidationResult(Stream containerStream, Validation flags);
}