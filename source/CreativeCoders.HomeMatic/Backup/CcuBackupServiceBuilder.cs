using System.Net;
using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Backup;

/// <summary>
/// Builds <see cref="CcuBackupService"/> instances configured for a specific CCU host and credentials.
/// </summary>
public class CcuBackupServiceBuilder(IHttpClientFactory httpClientFactory) : ICcuBackupServiceBuilder
{
    private readonly IHttpClientFactory _httpClientFactory = Ensure.NotNull(httpClientFactory);

    private string? _host;

    private NetworkCredential? _credential;

    /// <inheritdoc />
    public ICcuBackupServiceBuilder ForHost(string host)
    {
        Ensure.IsNotNullOrWhitespace(host);

        _host = host;

        return this;
    }

    /// <inheritdoc />
    public ICcuBackupServiceBuilder WithCredentials(NetworkCredential credential)
    {
        _credential = Ensure.NotNull(credential);

        return this;
    }

    /// <inheritdoc />
    public ICcuBackupService Build()
    {
        if (_host is null)
        {
            throw new InvalidOperationException("No host specified.");
        }

        if (_credential is null)
        {
            throw new InvalidOperationException("No credentials specified.");
        }

        var baseUrl = new UriBuilder
        {
            Scheme = "http",
            Host = _host
        }.Uri;

        var httpClient = _httpClientFactory.CreateClient("CcuBackup");

        return new CcuBackupService(httpClient, baseUrl, _credential);
    }
}
