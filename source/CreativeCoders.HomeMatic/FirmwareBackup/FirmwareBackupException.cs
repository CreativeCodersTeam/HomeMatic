using System.Net;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Exception thrown when a firmware backup operation against a HomeMatic CCU fails.
/// </summary>
[PublicAPI]
public class FirmwareBackupException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="FirmwareBackupException"/>.
    /// </summary>
    /// <param name="message">A human-readable description of the failure.</param>
    public FirmwareBackupException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FirmwareBackupException"/> with an inner exception.
    /// </summary>
    /// <param name="message">A human-readable description of the failure.</param>
    /// <param name="innerException">The exception that caused the failure.</param>
    public FirmwareBackupException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FirmwareBackupException"/> describing an HTTP failure.
    /// </summary>
    /// <param name="message">A human-readable description of the failure.</param>
    /// <param name="statusCode">HTTP status code returned by the CCU.</param>
    /// <param name="responseBody">Optional truncated response body for diagnostics.</param>
    public FirmwareBackupException(string message, HttpStatusCode statusCode, string? responseBody = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the CCU, if available.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets a (possibly truncated) snippet of the CCU response body for diagnostics.
    /// </summary>
    public string? ResponseBody { get; }
}
