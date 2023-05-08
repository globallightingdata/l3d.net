namespace L3D.Net.Abstract;

public abstract class ValidationHint
{
    public abstract Severity Severity { get; }
    public abstract string Message { get; }
}