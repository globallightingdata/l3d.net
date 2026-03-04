using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Abstract;

public interface IValidator
{
    public IEnumerable<ValidationHint> ValidateContainer(string containerPath, Validation flags);

    public IEnumerable<ValidationHint> ValidateContainer(byte[] containerBytes, Validation flags);

    public IEnumerable<ValidationHint> ValidateContainer(Stream containerStream, Validation flags);

    public ValidationResultContainer CreateValidationResult(string containerPath, Validation flags);

    public ValidationResultContainer CreateValidationResult(byte[] containerBytes, Validation flags);

    public ValidationResultContainer CreateValidationResult(Stream containerStream, Validation flags);
}