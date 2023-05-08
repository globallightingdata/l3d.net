namespace L3D.Net.Abstract;

public class L3DContentValidationHint : ValidationHint
{
    public override Severity Severity => Severity.Error;

    public override string Message => ErrorMessages.InvalidL3DContent;

    public string AdditionalInfo { get; }

    public L3DContentValidationHint(string additionalInfo)
    {
        AdditionalInfo = additionalInfo;
    }
}