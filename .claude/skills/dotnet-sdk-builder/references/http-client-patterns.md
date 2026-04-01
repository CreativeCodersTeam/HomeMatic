# HTTP Client Patterns for .NET SDK Libraries

## Typed HTTP Client Implementation

Always implement clients as typed HTTP clients registered via `AddHttpClient<TClient, TInterface>()`.

```csharp
internal sealed class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<GitHubOptions> _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public GitHubClient(HttpClient httpClient, IOptions<GitHubOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public async Task<Repository> GetRepositoryAsync(
        string owner,
        string repo,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(repo);

        var response = await _httpClient
            .GetAsync($"repos/{owner}/{repo}", cancellationToken)
            .ConfigureAwait(false);

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsync<Repository>(_jsonOptions, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new GitHubException("Unexpected null response body.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new GitHubAuthenticationException(response.StatusCode, body),
            HttpStatusCode.NotFound     => new GitHubNotFoundException(response.StatusCode, body),
            HttpStatusCode.TooManyRequests => new GitHubRateLimitException(response.StatusCode, body),
            _ => new GitHubException(response.StatusCode, body),
        };
    }
}
```

**Rules:**
- Mark the class `internal sealed` — consumers use the interface.
- Inject `HttpClient` (configured by `IHttpClientFactory`) and `IOptions<T>`.
- Use `ConfigureAwait(false)` on all awaits.
- Validate parameters with `ArgumentException.ThrowIfNullOrWhiteSpace` (.NET 7+) or `ArgumentNullException.ThrowIfNull`.
- All error mapping goes through a single `EnsureSuccessAsync` helper.

## Authentication Header Configuration

Configure auth in the `ConfigureHttpClient` delegate inside the extension method — not inside the client class:

```csharp
.ConfigureHttpClient((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<GitHubOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = options.Timeout;

    if (!string.IsNullOrWhiteSpace(options.AccessToken))
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.AccessToken);
});
```

For APIs that use API key headers:

```csharp
if (!string.IsNullOrWhiteSpace(options.ApiKey))
    client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
```

## Typed Exceptions

### Base Exception

```csharp
/// <summary>The exception thrown when the GitHub API returns an error response.</summary>
public class GitHubException : Exception
{
    /// <summary>The HTTP status code returned by the API.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>The raw response body returned by the API, if available.</summary>
    public string? ResponseBody { get; }

    public GitHubException(string message)
        : base(message) { }

    public GitHubException(HttpStatusCode statusCode, string? responseBody = null)
        : base($"GitHub API returned {(int)statusCode} {statusCode}.")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public GitHubException(HttpStatusCode statusCode, string? responseBody, Exception inner)
        : base($"GitHub API returned {(int)statusCode} {statusCode}.", inner)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
```

### Specific Exception Types

Create one subclass per distinct error scenario:

```csharp
/// <summary>Thrown when the GitHub API returns 401 Unauthorized.</summary>
public sealed class GitHubAuthenticationException : GitHubException
{
    public GitHubAuthenticationException(HttpStatusCode statusCode, string? responseBody = null)
        : base(statusCode, responseBody) { }
}

/// <summary>Thrown when the requested resource does not exist (404).</summary>
public sealed class GitHubNotFoundException : GitHubException
{
    public GitHubNotFoundException(HttpStatusCode statusCode, string? responseBody = null)
        : base(statusCode, responseBody) { }
}

/// <summary>Thrown when the GitHub API rate limit has been exceeded (429).</summary>
public sealed class GitHubRateLimitException : GitHubException
{
    /// <summary>The UTC time at which the rate limit resets, if provided by the API.</summary>
    public DateTimeOffset? ResetAt { get; init; }

    public GitHubRateLimitException(HttpStatusCode statusCode, string? responseBody = null)
        : base(statusCode, responseBody) { }
}
```

**Rules:**
- Base exception is `public class` (unsealed) — callers can catch the base type.
- Subtypes are `public sealed class`.
- Always include `StatusCode` and `ResponseBody` on the base type.
- Add domain-specific properties to subtypes (e.g., `ResetAt` for rate limit).

## Resilience (Polly via Microsoft.Extensions.Http.Resilience) {#resilience}

Only add if the user confirms resilience is desired.

```xml
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="*" />
```

```csharp
private static IHttpClientBuilder AddGitHubCore(this IServiceCollection services)
{
    return services
        .AddHttpClient<IGitHubClient, GitHubClient>()
        .ConfigureHttpClient((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<GitHubOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        })
        .AddStandardResilienceHandler(options =>
        {
            // Optional: customize defaults
            options.Retry.MaxRetryAttempts = 3;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        });
}
```

`AddStandardResilienceHandler()` configures retry (with exponential backoff), circuit breaker, and timeout by default. Customize only when the defaults are insufficient.

## JSON Serialization

Use `System.Text.Json` (built-in). Configure a shared `JsonSerializerOptions` instance — do not create new instances per call:

```csharp
private static readonly JsonSerializerOptions JsonOptions = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,  // for snake_case APIs
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
};
```

For APIs that use camelCase (most REST APIs): use `JsonNamingPolicy.CamelCase`.
For APIs that use PascalCase: omit `PropertyNamingPolicy`.

## Request/Response Models (DTOs)

```csharp
/// <summary>Represents a GitHub repository.</summary>
public sealed class Repository
{
    /// <summary>The unique identifier of the repository.</summary>
    public long Id { get; init; }

    /// <summary>The name of the repository.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The full qualified name including owner (e.g., "owner/repo").</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Indicates whether the repository is private.</summary>
    public bool Private { get; init; }

    /// <summary>The URL of the repository on GitHub.</summary>
    public string HtmlUrl { get; init; } = string.Empty;
}
```

**Rules:**
- Use `init` setters for immutable DTOs.
- Initialize string properties to `string.Empty` to avoid nulls.
- Use `sealed` for response models.
- Use `JsonPropertyName` attribute only when the JSON key differs from the C# name AND a naming policy cannot handle it.

## Pagination Pattern (if needed)

For paginated APIs, return `IAsyncEnumerable<T>`:

```csharp
public interface IGitHubClient
{
    IAsyncEnumerable<Repository> GetAllRepositoriesAsync(
        string username,
        CancellationToken cancellationToken = default);
}
```

Implementation uses `yield return` with async page fetching.
