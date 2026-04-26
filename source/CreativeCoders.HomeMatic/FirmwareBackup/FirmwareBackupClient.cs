using CreativeCoders.Core;
using CreativeCoders.HomeMatic.FirmwareBackup.Internal;
using System.IO.Abstractions;

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
    private readonly IFileSystem _fileSystem;

    internal FirmwareBackupClient(
        ICcuSessionClient sessionClient,
        IFirmwareBackupDownloader downloader,
        FirmwareBackupOptions options,
        IFileSystem fileSystem)
    {
        _sessionClient = Ensure.NotNull(sessionClient);
        _downloader = Ensure.NotNull(downloader);
        _options = Ensure.NotNull(options);
        _fileSystem = Ensure.NotNull(fileSystem);
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
    public async Task<string> CreateBackupToFileAsync(string targetFilePath,
        CancellationToken cancellationToken = default)
    {
        Ensure.IsNotNullOrWhitespace(targetFilePath);

        var backup = await CreateBackupAsync(cancellationToken).ConfigureAwait(false);
        await using var backup1 = backup.ConfigureAwait(false);

        var resolvedPath = ResolveFilePath(targetFilePath, backup.FileName);

        var directory = _fileSystem.Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            _fileSystem.Directory.CreateDirectory(directory);
        }

        var fileStream = _fileSystem.File.Create(resolvedPath);
        await using var stream = fileStream.ConfigureAwait(false);
        await backup.Content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

        return resolvedPath;
    }

    private string ResolveFilePath(string targetFilePath, string suggestedFileName)
    {
        if (_fileSystem.Directory.Exists(targetFilePath) ||
            targetFilePath.EndsWith(_fileSystem.Path.DirectorySeparatorChar) ||
            targetFilePath.EndsWith(_fileSystem.Path.AltDirectorySeparatorChar))
        {
            return _fileSystem.Path.Combine(targetFilePath, suggestedFileName);
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
