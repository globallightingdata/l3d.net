using System;
#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace L3D.Net.Exceptions;

[Serializable]
public class ModelParseException : L3DNetBaseException
{
    public ModelParseException()
    {
    }

    public ModelParseException(string message) : base(message)
    {
    }

    public ModelParseException(string message, Exception innerException) : base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    protected ModelParseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#endif
}