using System;
using CreativeCoders.HomeMatic.XmlRpc.Exceptions;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.XmlRpc.Server;

/// <summary>
/// Represents the error that occurs when a CCU event server cannot listen on its URL.
/// </summary>
[PublicAPI]
public sealed class CcuEventServerStartException : HomeMaticException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CcuEventServerStartException"/> class with the failure reason, a
    /// specified error message and a reference to the exception that caused it.
    /// </summary>
    /// <param name="reason">The reason why the server cannot listen.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the start to fail.</param>
    public CcuEventServerStartException(
        CcuEventServerStartFailure reason,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets the reason why the server cannot listen.
    /// </summary>
    /// <value>One of the enumeration values that specifies the failure reason.</value>
    public CcuEventServerStartFailure Reason { get; }
}

/// <summary>
/// Specifies why a CCU event server cannot listen on its URL.
/// </summary>
[PublicAPI]
public enum CcuEventServerStartFailure
{
    /// <summary>
    /// The cause is not one of the other values.
    /// </summary>
    Other,

    /// <summary>
    /// The port is already in use by another process.
    /// </summary>
    AddressInUse,

    /// <summary>
    /// The current user is not allowed to listen on the URL.
    /// </summary>
    AccessDenied
}
