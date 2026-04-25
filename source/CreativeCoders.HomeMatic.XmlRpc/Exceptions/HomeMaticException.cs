using System;

namespace CreativeCoders.HomeMatic.XmlRpc.Exceptions;

/// <summary>
/// Serves as the base class for all HomeMatic-specific exceptions.
/// </summary>
public abstract class HomeMaticException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeMaticException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    protected HomeMaticException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
