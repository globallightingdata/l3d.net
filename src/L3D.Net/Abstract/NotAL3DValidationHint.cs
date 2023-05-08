namespace L3D.Net.Abstract;

// ReSharper disable once InconsistentNaming
public sealed class NotAL3DValidationHint : ValidationHint
{
    public override Severity Severity => Severity.Error;

    public override string Message => ErrorMessages.NotAL3D;
}