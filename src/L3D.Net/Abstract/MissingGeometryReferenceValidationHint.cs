namespace L3D.Net.Abstract;

public sealed class MissingGeometryReferenceValidationHint : ValidationHint
{
    public override Severity Severity => Severity.Error;

    public override string Message => ErrorMessages.MissingGeometryReference;

    public string AdditionalInfo { get; }

    public MissingGeometryReferenceValidationHint(string additionalInfo)
    {
        AdditionalInfo = additionalInfo;
    }
}