using System;

namespace L3D.Net.Exceptions;

public class ModelParseException : Exception
{
    public ModelParseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}