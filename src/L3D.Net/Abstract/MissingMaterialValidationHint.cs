namespace L3D.Net.Abstract;

public class MissingMaterialValidationHint : ValidationHint
{
    public override Severity Severity => Severity.Warning;

    public override string Message => ErrorMessages.MissingMaterial;

    public string AdditionalInfo { get; }

    public MissingMaterialValidationHint(string additionalInfo)
    {
        AdditionalInfo = additionalInfo;
    }
}