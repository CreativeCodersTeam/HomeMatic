using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Exception thrown when a downloaded firmware backup is not a valid HomeMatic CCU backup.
/// </summary>
/// <remarks>
/// Derives from <see cref="FirmwareBackupException"/> so existing handlers that catch firmware
/// backup failures also catch an invalid backup. A backup is considered invalid when it cannot be
/// read as the expected HomeMatic CCU backup archive (for example, missing or empty
/// <c>signature</c>, <c>usr_local.tar.gz</c>, <c>firmware_version</c> or <c>key_index</c> entries,
/// or entries whose content is malformed).
/// </remarks>
[PublicAPI]
public class InvalidFirmwareBackupException : FirmwareBackupException
{
    /// <summary>
    /// Initializes a new instance of <see cref="InvalidFirmwareBackupException"/>.
    /// </summary>
    /// <param name="message">A human-readable description of why the backup is invalid.</param>
    public InvalidFirmwareBackupException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InvalidFirmwareBackupException"/> with an inner exception.
    /// </summary>
    /// <param name="message">A human-readable description of why the backup is invalid.</param>
    /// <param name="innerException">The exception that caused the validation to fail.</param>
    public InvalidFirmwareBackupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
