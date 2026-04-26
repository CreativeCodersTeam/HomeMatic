using CreativeCoders.Core;
using CreativeCoders.HomeMatic.FirmwareBackup.Internal;

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

    /// <summary>
    /// Initializes a new instance of <see cref="FirmwareBackupClientFactory"/>.
    /// </summary>
    /// <param name="httpClientFactory">Factory used to obtain the named HTTP client.</param>
    public FirmwareBackupClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = Ensure.NotNull(httpClientFactory);
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

        return new FirmwareBackupClient(sessionClient, downloader, options);
    }
}
