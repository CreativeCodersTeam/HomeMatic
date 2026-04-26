using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.FirmwareBackup.Internal;

/// <summary>
/// Default <see cref="IFirmwareBackupDownloader"/> that calls the CCU CGI backup endpoint.
/// </summary>
internal sealed class FirmwareBackupDownloader : IFirmwareBackupDownloader
{
    private const string DefaultFileName = "ccu_backup.sbk";

    private readonly HttpClient _httpClient;
    private readonly Uri _backupUrl;
    private readonly string _backupAction;

    public FirmwareBackupDownloader(HttpClient httpClient, Uri baseUrl, string backupCgiPath, string backupAction)
    {
        _httpClient = Ensure.NotNull(httpClient);
        Ensure.NotNull(baseUrl);
        Ensure.IsNotNullOrWhitespace(backupCgiPath);

        _backupUrl = new Uri(baseUrl, backupCgiPath);
        _backupAction = Ensure.IsNotNullOrWhitespace(backupAction);
    }

    public async Task<FirmwareBackupDownloadResult> DownloadAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        Ensure.IsNotNullOrWhitespace(sessionId);

        var requestUri = BuildRequestUri(sessionId);

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await SafeReadAsync(response, cancellationToken).ConfigureAwait(false);

            response.Dispose();
            request.Dispose();

            throw new FirmwareBackupException(
                $"Firmware backup download failed with HTTP status {(int)response.StatusCode}.",
                response.StatusCode,
                body);
        }

        var fileName = ResolveFileName(response);
        var contentLength = response.Content.Headers.ContentLength;

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return new FirmwareBackupDownloadResult(
            stream,
            fileName,
            contentLength,
            new HttpResources(request, response));
    }

    private static string ResolveFileName(HttpResponseMessage response)
    {
        var disposition = response.Content.Headers.ContentDisposition;

        var fileName = disposition?.FileNameStar ?? disposition?.FileName;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return DefaultFileName;
        }

        return fileName!.Trim('"');
    }

    private Uri BuildRequestUri(string sessionId)
    {
        var encodedSid = Uri.EscapeDataString($"@{sessionId}@");
        var encodedAction = Uri.EscapeDataString(_backupAction);

        var separator = string.IsNullOrEmpty(_backupUrl.Query) ? "?" : "&";
        var query = $"{separator}sid={encodedSid}&action={encodedAction}";

        return new Uri(_backupUrl + query);
    }

    private static async Task<string?> SafeReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return body.Length <= 512 ? body : body[..512];
        }
        catch
        {
            return null;
        }
    }

    private sealed class HttpResources(HttpRequestMessage request, HttpResponseMessage response) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            response.Dispose();
            request.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
