using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Backup;

/// <summary>
/// Creates system backups of a HomeMatic CCU, equivalent to the backup function
/// available in the CCU Web UI.
/// </summary>
[PublicAPI]
public interface ICcuBackupService
{
    /// <summary>
    /// Asynchronously creates a system backup of the CCU and returns it as a stream.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that yields a <see cref="Stream"/> containing the backup archive (.tar.gz). The caller is responsible for disposing the stream.</returns>
    Task<Stream> CreateBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously creates a system backup of the CCU and saves it to the specified file path.
    /// </summary>
    /// <param name="outputFilePath">The file path where the backup archive (.tar.gz) will be saved.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveBackupAsync(string outputFilePath, CancellationToken cancellationToken = default);
}
