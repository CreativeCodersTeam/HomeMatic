namespace CreativeCoders.HomeMatic.FirmwareBackup.Internal;

/// <summary>
/// Verifies that a downloaded backup is a valid HomeMatic CCU backup archive.
/// </summary>
/// <remarks>
/// This is a structural integrity check only: it confirms the archive contains the expected,
/// non-empty entries. The <c>signature</c> entry is verified to be present and non-empty; its
/// contents are not cryptographically validated, so a passing result is not proof of authenticity.
/// </remarks>
internal interface ICcuBackupVerifier
{
    /// <summary>
    /// Verifies that the given stream contains a valid HomeMatic CCU backup.
    /// </summary>
    /// <remarks>
    /// A backup is considered valid when it is a tar archive containing a <c>signature</c> entry
    /// with 32 hexadecimal characters, a <c>usr_local.tar.gz</c> entry that is a gzip compressed
    /// tar archive with <c>usr/local</c> content, a <c>firmware_version</c> entry of the form
    /// <c>VERSION=&lt;version&gt;</c> and a <c>key_index</c> entry containing a non-negative
    /// integer. The signature is not cryptographically verified.
    /// </remarks>
    /// <param name="content">
    /// The stream containing the backup payload. If it is seekable, it is rewound to the beginning
    /// before reading; otherwise it is read from its current position. The stream is left open and
    /// its position is undefined after the call.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when verification succeeds.</returns>
    /// <exception cref="InvalidFirmwareBackupException">
    /// Thrown when the content is not a valid HomeMatic CCU backup.
    /// </exception>
    Task VerifyAsync(Stream content, CancellationToken cancellationToken = default);
}
