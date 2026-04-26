namespace CreativeCoders.HomeMatic.FirmwareBackup.Internal;

/// <summary>
/// Triggers backup creation on a HomeMatic CCU and downloads the resulting <c>.sbk</c> file.
/// </summary>
internal interface IFirmwareBackupDownloader
{
    /// <summary>
    /// Downloads the firmware backup using the given CCU session id.
    /// </summary>
    /// <param name="sessionId">Session id obtained from a prior login.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The download result containing stream, metadata and HTTP resources.</returns>
    Task<FirmwareBackupDownloadResult> DownloadAsync(string sessionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Internal result type produced by <see cref="IFirmwareBackupDownloader"/>.
/// </summary>
/// <param name="Content">The backup payload stream.</param>
/// <param name="FileName">The file name reported by the CCU (or a default).</param>
/// <param name="ContentLength">Optional content length in bytes.</param>
/// <param name="HttpResources">Disposable wrapping the underlying HTTP request/response.</param>
internal sealed record FirmwareBackupDownloadResult(
    Stream Content,
    string FileName,
    long? ContentLength,
    IAsyncDisposable HttpResources);

