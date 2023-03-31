namespace L3D.Net.Abstract
{
    public sealed class NotAL3DValidationHint : ValidationHint
    {
        public override Severity Severity => Severity.Error;

        public override string Message => ErrorMessages.NotAL3D;
    }
}
