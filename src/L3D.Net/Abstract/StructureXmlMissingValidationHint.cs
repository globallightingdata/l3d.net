namespace L3D.Net.Abstract
{
    public sealed class StructureXmlMissingValidationHint : ValidationHint
    {
        public override Severity Severity => Severity.Error;

        public override string Message => ErrorMessages.StructureXmlMissing;
    }
}
