using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;
using System;

namespace L3D.Net.Mapper.V0_11_0;

public class ShapeMapper : DtoMapperBase<Shape, ShapeDto>
{
    public static readonly ShapeMapper Instance = new();

    protected override Shape ConvertData(ShapeDto element) => element switch
    {
        CircleDto circle => new Circle
        {
            Diameter = circle.Diameter
        },
        RectangleDto rectangle => new Rectangle
        {
            SizeX = rectangle.SizeX,
            SizeY = rectangle.SizeY
        },
        _ => throw new ArgumentOutOfRangeException(nameof(element))
    };

    protected override ShapeDto ConvertData(Shape element) => element switch
    {
        Circle circle => new CircleDto
        {
            Diameter = circle.Diameter
        },
        Rectangle rectangle => new RectangleDto
        {
            SizeX = rectangle.SizeX,
            SizeY = rectangle.SizeY
        },
        _ => throw new ArgumentOutOfRangeException(nameof(element))
    };
}