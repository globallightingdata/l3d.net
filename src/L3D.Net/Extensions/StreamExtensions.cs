using System.IO;

namespace L3D.Net.Extensions;

public static class StreamExtensions
{
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue"></see> elements.</exception>
    public static byte[] ToArray(this Stream input)
    {
        var buffer = new byte[input.Length];
        input.Seek(0, SeekOrigin.Begin);
        _ = input.Read(buffer, 0, buffer.Length);
        return buffer;
    }
}