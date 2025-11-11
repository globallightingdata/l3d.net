namespace L3D.Net.Internal.Abstract;

public class FileInformation
{
    public string Name { get; set; } = string.Empty;

    public byte[]? Data { get; set; }

    public FileStatus Status { get; set; }
}