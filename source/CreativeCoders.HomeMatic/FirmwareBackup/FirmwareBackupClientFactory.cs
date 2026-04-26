using CreativeCoders.Core;
using CreativeCoders.HomeMatic.FirmwareBackup.Internal;
using System.IO.Abstractions;

namespace CreativeCoders.HomeMatic.FirmwareBackup;

/// <summary>
/// Default <see cref="IFirmwareBackupClientFactory"/> implementation. Resolves the configured
/// <see cref="HttpClient"/> via <see cref="IHttpClientFactory"/> and wires up a
/// <see cref="FirmwareBackupClient"/> with its internal collaborators.
/// </summary>
public sealed class FirmwareBackupClientFactory : IFirmwareBackupClientFactory
{
    /// <summary>
    /// Name of the named <see cref="HttpClient"/> registered for firmware backup operations.
    /// </summary>
    public const string HttpClientName = "CreativeCoders.HomeMatic.FirmwareBackup";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="FirmwareBackupClientFactory"/>.
    /// </summary>
    /// <param name="httpClientFactory">Factory used to obtain the named HTTP client.</param>
    /// <param name="fileSystem">File system abstraction used by created clients.</param>
    public FirmwareBackupClientFactory(IHttpClientFactory httpClientFactory, IFileSystem fileSystem)
    {
        _httpClientFactory = Ensure.NotNull(httpClientFactory);
        _fileSystem = Ensure.NotNull(fileSystem);
    }

    /// <inheritdoc />
    public IFirmwareBackupClient Create(FirmwareBackupOptions options)
    {
        Ensure.NotNull(options);

        var httpClient = _httpClientFactory.CreateClient(HttpClientName);
        httpClient.Timeout = options.Timeout;

        var sessionClient = new CcuSessionClient(httpClient, options.BaseUrl, options.JsonRpcPath);
        var downloader = new FirmwareBackupDownloader(
            httpClient,
            options.BaseUrl,
            options.BackupCgiPath,
            options.BackupAction);

        return new FirmwareBackupClient(sessionClient, downloader, options, _fileSystem);
    }
}
