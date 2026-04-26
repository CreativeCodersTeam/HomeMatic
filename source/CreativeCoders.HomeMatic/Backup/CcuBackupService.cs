using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.Backup;

/// <summary>
/// Creates system backups of a HomeMatic CCU by authenticating via JSON-RPC and downloading
/// the backup archive over HTTP.
/// </summary>
public class CcuBackupService : ICcuBackupService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly Uri _baseUrl;
    private readonly NetworkCredential _credential;

    /// <summary>
    /// Initializes a new instance of the <see cref="CcuBackupService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for communication with the CCU.</param>
    /// <param name="baseUrl">The base URL of the CCU (e.g. <c>http://192.168.1.100</c>).</param>
    /// <param name="credential">The credentials used to authenticate against the CCU.</param>
    public CcuBackupService(HttpClient httpClient, Uri baseUrl, NetworkCredential credential)
    {
        _httpClient = Ensure.NotNull(httpClient);
        _baseUrl = Ensure.NotNull(baseUrl);
        _credential = Ensure.NotNull(credential);
    }

    /// <inheritdoc />
    public async Task<Stream> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        var sessionId = await LoginAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var backupUrl = new Uri(_baseUrl, $"/config/cp_security.cgi?sid={sessionId}&action=create_backup");

            using var backupRequest = new HttpRequestMessage(HttpMethod.Get, backupUrl);
            backupRequest.Headers.Add("Cookie", $"SID={sessionId}");

            var response = await _httpClient.SendAsync(backupRequest, HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"CCU backup failed: {errorBody.Trim()}");
            }

            var memoryStream = new MemoryStream();

            await response.Content.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);

            memoryStream.Position = 0;

            return memoryStream;
        }
        finally
        {
            await LogoutAsync(sessionId, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task SaveBackupAsync(string outputFilePath, CancellationToken cancellationToken = default)
    {
        Ensure.IsNotNullOrWhitespace(outputFilePath);

        await using var backupStream = await CreateBackupAsync(cancellationToken).ConfigureAwait(false);
        await using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true);

        await backupStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> LoginAsync(CancellationToken cancellationToken)
    {
        var request = new JsonRpcRequest
        {
            Method = "Session.login",
            Params = new Dictionary<string, string>
            {
                ["username"] = _credential.UserName,
                ["password"] = _credential.Password
            }
        };

        var apiUrl = new Uri(_baseUrl, "/api/homematic.cgi");
        var content = new StringContent(JsonSerializer.Serialize(request, JsonOptions),
            System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(apiUrl, content, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var loginResponse = await JsonSerializer.DeserializeAsync<JsonRpcResponse>(responseStream, JsonOptions,
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrEmpty(loginResponse?.Result))
        {
            throw new InvalidOperationException("CCU login failed: no session ID received.");
        }

        return loginResponse.Result;
    }

    private async Task LogoutAsync(string sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var request = new JsonRpcRequest
            {
                Method = "Session.logout",
                Params = new Dictionary<string, string>
                {
                    ["_session_id_"] = sessionId
                }
            };

            var apiUrl = new Uri(_baseUrl, "/api/homematic.cgi");
            var content = new StringContent(JsonSerializer.Serialize(request, JsonOptions),
                System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Best-effort logout — do not let logout failures mask the original operation result.
        }
    }

    private record JsonRpcRequest
    {
        [JsonPropertyName("method")]
        public required string Method { get; init; }

        [JsonPropertyName("params")]
        public required Dictionary<string, string> Params { get; init; }
    }

    private record JsonRpcResponse
    {
        [JsonPropertyName("result")]
        public string? Result { get; init; }
    }
}
