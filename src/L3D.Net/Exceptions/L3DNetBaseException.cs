using System;
#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace L3D.Net.Exceptions;

[Serializable]
public abstract class L3DNetBaseException : Exception
{
    protected L3DNetBaseException()
    {
    }

    protected L3DNetBaseException(string message) : base(message)
    {
    }

    protected L3DNetBaseException(string message, Exception innerException) : base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    protected L3DNetBaseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif
}