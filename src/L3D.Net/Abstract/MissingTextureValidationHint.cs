namespace L3D.Net.Abstract;

public class MissingTextureValidationHint : ValidationHint
{
    public override Severity Severity => Severity.Warning;

    public override string Message => ErrorMessages.MissingTexture;

    public string AdditionalInfo { get; }

    public MissingTextureValidationHint(string additionalInfo)
    {
        AdditionalInfo = additionalInfo;
    }
}