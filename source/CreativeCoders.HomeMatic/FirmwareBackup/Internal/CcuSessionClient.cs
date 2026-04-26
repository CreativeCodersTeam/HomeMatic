using System.Text;
using System.Text.Json;
using CreativeCoders.Core;

namespace CreativeCoders.HomeMatic.FirmwareBackup.Internal;

/// <summary>
/// Default <see cref="ICcuSessionClient"/> implementation that talks to the CCU JSON-RPC endpoint via
/// <see cref="HttpClient"/>.
/// </summary>
internal sealed class CcuSessionClient : ICcuSessionClient
{
    private readonly HttpClient _httpClient;
    private readonly Uri _jsonRpcUrl;

    public CcuSessionClient(HttpClient httpClient, Uri baseUrl, string jsonRpcPath)
    {
        _httpClient = Ensure.NotNull(httpClient);
        Ensure.NotNull(baseUrl);
        Ensure.IsNotNullOrWhitespace(jsonRpcPath);

        _jsonRpcUrl = new Uri(baseUrl, jsonRpcPath);
    }

    public async Task<string> LoginAsync(string userName, string password,
        CancellationToken cancellationToken = default)
    {
        Ensure.IsNotNullOrWhitespace(userName);
        Ensure.NotNull(password);

        var payload = new
        {
            version = "1.1",
            method = "Session.login",
            @params = new { username = userName, password }
        };

        var response = await PostJsonRpcAsync(payload, cancellationToken).ConfigureAwait(false);

        var sessionId = ReadResultString(response);

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new FirmwareBackupException(
                "CCU did not return a session id. Please verify the credentials.");
        }

        return sessionId;
    }

    public async Task LogoutAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return;
        }

        var payload = new
        {
            version = "1.1",
            method = "Session.logout",
            @params = new { _session_id_ = sessionId }
        };

        try
        {
            await PostJsonRpcAsync(payload, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Logout is best-effort; do not propagate errors when releasing a session.
        }
    }

    private async Task<string> PostJsonRpcAsync(object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(HttpMethod.Post, _jsonRpcUrl);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new FirmwareBackupException(
                $"JSON-RPC call against CCU failed with HTTP status {(int)response.StatusCode}.",
                response.StatusCode,
                Truncate(body));
        }

        return body;
    }

    private static string? ReadResultString(string jsonResponse)
    {
        using var document = JsonDocument.Parse(jsonResponse);
        var root = document.RootElement;

        if (root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind != JsonValueKind.Null)
        {
            var message = errorElement.TryGetProperty("message", out var msgEl) &&
                          msgEl.ValueKind == JsonValueKind.String
                ? msgEl.GetString()
                : errorElement.ToString();

            throw new FirmwareBackupException($"CCU returned a JSON-RPC error: {message}");
        }

        if (!root.TryGetProperty("result", out var resultElement))
        {
            return null;
        }

        return resultElement.ValueKind == JsonValueKind.String ? resultElement.GetString() : resultElement.ToString();
    }

    private static string Truncate(string value)
    {
        const int maxLength = 512;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
