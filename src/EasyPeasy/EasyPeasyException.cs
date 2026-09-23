namespace EasyPeasy;

/// <summary>
/// Thrown when a client is used in a way EasyPeasy cannot support, for example when no
/// <see cref="IMediaTypeHandler"/> is registered for a response's media type.
/// </summary>
public class EasyPeasyException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="EasyPeasyException"/> class.</summary>
    public EasyPeasyException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="EasyPeasyException"/> class.</summary>
    /// <param name="message">The error message.</param>
    public EasyPeasyException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="EasyPeasyException"/> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public EasyPeasyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
