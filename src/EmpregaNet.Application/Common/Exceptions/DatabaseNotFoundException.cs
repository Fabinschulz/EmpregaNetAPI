
using System.Runtime.Serialization;

namespace Common.Exceptions;

[Serializable]
public class DatabaseNotFoundException : Exception
{
    public DatabaseNotFoundException()
    {
    }

    public DatabaseNotFoundException(string message)
        : base(message)
    {
    }

    public DatabaseNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    // Implement custom serialization
    protected DatabaseNotFoundException(SerializationInfo info, StreamingContext context)
    {
    }
}

