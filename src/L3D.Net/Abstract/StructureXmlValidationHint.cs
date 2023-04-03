namespace L3D.Net.Abstract
{
    public sealed class StructureXmlValidationHint : ValidationHint
    {
        public override Severity Severity { get; }

        public override string Message { get; }

        public string AdditionalInfo { get; } = string.Empty;

        public StructureXmlValidationHint(string message)
        {
            Message = message;
            Severity = Severity.Error;
        }

        public StructureXmlValidationHint(string message, string additionalInfo)
        {
            Message = message;
            AdditionalInfo = additionalInfo;
            Severity = Severity.Error;
        }

        public StructureXmlValidationHint(string message, string additionalInfo, Severity severity)
        {
            Message = message;
            AdditionalInfo = additionalInfo;
            Severity = severity;
        }
    }
}
