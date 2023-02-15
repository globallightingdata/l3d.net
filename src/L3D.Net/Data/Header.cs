using System;

namespace L3D.Net.Data;

internal class Header
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string CreatedWithApplication { get; set; }
    public DateTime CreationTimeCode { get; set; } = DateTime.UtcNow;
}