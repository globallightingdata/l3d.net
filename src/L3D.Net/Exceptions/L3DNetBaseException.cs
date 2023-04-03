using System;
using System.Runtime.Serialization;

namespace L3D.Net.Exceptions
{
    [Serializable]
    public abstract class L3DNetBaseException : Exception
    {
        protected L3DNetBaseException()
        {
        }

        protected L3DNetBaseException(string message) : base(message)
        {
        }

        protected L3DNetBaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected L3DNetBaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
