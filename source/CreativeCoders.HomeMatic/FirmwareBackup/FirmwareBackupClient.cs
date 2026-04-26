using CreativeCoders.Core;
using CreativeCoders.HomeMatic.FirmwareBackup.Internal;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Default <see cref="IFirmwareBackupClient"/> implementation. Orchestrates login, backup download
/// and logout against a HomeMatic CCU.
/// </summary>
public sealed class FirmwareBackupClient : IFirmwareBackupClient
{
    private readonly ICcuSessionClient _sessionClient;
    private readonly IFirmwareBackupDownloader _downloader;
    private readonly FirmwareBackupOptions _options;

    internal FirmwareBackupClient(
        ICcuSessionClient sessionClient,
        IFirmwareBackupDownloader downloader,
        FirmwareBackupOptions options)
    {
        _sessionClient = Ensure.NotNull(sessionClient);
        _downloader = Ensure.NotNull(downloader);
        _options = Ensure.NotNull(options);
    }

    /// <inheritdoc />
    public async Task<FirmwareBackupResult> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        var sessionId = await _sessionClient
            .LoginAsync(_options.Credential.UserName, _options.Credential.Password, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var download = await _downloader.DownloadAsync(sessionId, cancellationToken).ConfigureAwait(false);

            return new FirmwareBackupResult(
                download.Content,
                download.FileName,
                download.ContentLength,
                download.HttpResources,
                new LogoutDisposable(_sessionClient, sessionId));
        }
        catch
        {
            await _sessionClient.LogoutAsync(sessionId, CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> CreateBackupToFileAsync(string targetFilePath, CancellationToken cancellationToken = default)
    {
        Ensure.IsNotNullOrWhitespace(targetFilePath);

        await using var backup = await CreateBackupAsync(cancellationToken).ConfigureAwait(false);

        var resolvedPath = ResolveFilePath(targetFilePath, backup.FileName);

        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = File.Create(resolvedPath);
        await backup.Content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

        return resolvedPath;
    }

    private static string ResolveFilePath(string targetFilePath, string suggestedFileName)
    {
        if (Directory.Exists(targetFilePath))
        {
            return Path.Combine(targetFilePath, suggestedFileName);
        }

        if (targetFilePath.EndsWith(Path.DirectorySeparatorChar) ||
            targetFilePath.EndsWith(Path.AltDirectorySeparatorChar))
        {
            return Path.Combine(targetFilePath, suggestedFileName);
        }

        return targetFilePath;
    }

    private sealed class LogoutDisposable(ICcuSessionClient sessionClient, string sessionId) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await sessionClient.LogoutAsync(sessionId, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
