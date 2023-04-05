namespace L3D.Net.Abstract
{
    public class UnusedFileValidationHint : ValidationHint
    {
        public override Severity Severity => Severity.Warning;

        public override string Message => ErrorMessages.UnusedFile;

        public string AdditionalInfo { get; }

        public UnusedFileValidationHint(string additionalInfo)
        {
            AdditionalInfo = additionalInfo;
        }
    }
}
