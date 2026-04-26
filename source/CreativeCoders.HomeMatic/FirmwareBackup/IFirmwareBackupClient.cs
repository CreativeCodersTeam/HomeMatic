using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Client for creating and downloading a firmware backup of a HomeMatic CCU.
/// </summary>
[PublicAPI]
public interface IFirmwareBackupClient
{
    /// <summary>
    /// Creates a firmware backup on the CCU and returns it as a stream wrapped in a
    /// <see cref="FirmwareBackupResult"/>. The caller is responsible for disposing the result.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The backup result containing stream and metadata.</returns>
    Task<FirmwareBackupResult> CreateBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a firmware backup on the CCU and writes it to the given target file.
    /// </summary>
    /// <param name="targetFilePath">
    /// Either an absolute file path or a directory path. If a directory is given, the file name
    /// reported by the CCU is appended.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The full path of the written backup file.</returns>
    Task<string> CreateBackupToFileAsync(string targetFilePath, CancellationToken cancellationToken = default);
}
