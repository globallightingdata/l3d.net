using System.Collections.Generic;

namespace L3D.Net.API.Dto;

public class FaceDto
{
    public List<FaceVertexDto> Vertices { get; set; }
    public int MaterialIndex { get; set; }
}