using System;

namespace L3D.Net.Data;

public class Header
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedWithApplication { get; set; } = string.Empty;
    public DateTime CreationTimeCode { get; set; } = DateTime.UtcNow;
}