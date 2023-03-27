using System;
using System.Runtime.Serialization;

namespace L3D.Net.Exceptions
{
    [Serializable]
    public class InvalidL3DException : L3DNetBaseException
    {
        public InvalidL3DException()
        {
        }

        public InvalidL3DException(string message) : base(message)
        {
        }

        public InvalidL3DException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidL3DException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
