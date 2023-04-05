namespace L3D.Net.Data;

public class LightEmittingPart : TransformablePart
{
    public Shape Shape { get; set; }
    public LuminousHeights? LuminousHeights { get; set; }

    public LightEmittingPart(Shape shape)
    {
        Shape = shape;
    }
}