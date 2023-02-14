namespace L3D.Net.Data;

internal class LuminousHeights
{
    public LuminousHeights(double c0, double c90, double c180, double c270)
    {
        C0 = c0;
        C90 = c90;
        C180 = c180;
        C270 = c270;
    }

    public double C0 { get; }
    public double C90 { get; }
    public double C180 { get; }
    public double C270 { get; }
}