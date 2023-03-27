using System;
using System.Runtime.Serialization;

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

    protected ModelParseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}