using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Events;

/// <summary>
/// Represents the error that occurs when no free local TCP port can be allocated for the callback endpoint.
/// </summary>
[PublicAPI]
public sealed class CallbackPortAllocationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallbackPortAllocationException"/> class with a specified error
    /// message and a reference to the exception that caused it.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the port allocation to fail.</param>
    public CallbackPortAllocationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
