namespace L3D.Net.Abstract
{
    public sealed class InvalidZipValidationHint : ValidationHint
    {
        public override Severity Severity => Severity.Error;

        public override string Message => ErrorMessages.InvalidZip;
    }
}
