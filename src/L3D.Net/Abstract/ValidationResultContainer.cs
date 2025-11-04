using L3D.Net.Data;

namespace L3D.Net.Abstract;

public class ValidationResultContainer
{
    public ValidationHint[] ValidationHints { get; set; } = [];

    /// <summary>
    ///     Luminaire filled when required for validation
    /// </summary>
    public Luminaire? Luminaire { get; set; }
}