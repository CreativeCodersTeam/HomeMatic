# OpenAPI, Health Checks & Cross-Cutting Concerns

## OpenAPI / Swagger

- Use the built-in OpenAPI support (.NET 9+) or Swashbuckle/NSwag for earlier versions:

```csharp
builder.Services.AddOpenApi();
// ...
app.MapOpenApi();
```

- Enable XML comments in `.csproj` for automatic documentation:
  ```xml
  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>
  ```
- Use `[Tags]`, `[EndpointSummary]`, `[EndpointDescription]` for metadata
- Use `WithName()` and `WithTags()` on minimal API endpoints
- Use the `dotnet-xmldocs` skill for writing XML documentation comments

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(connectionString, name: "database")
    .AddRedis(redisConnectionString, name: "cache");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

- Separate liveness (`/health`) and readiness (`/health/ready`) endpoints
- Tag health checks for selective filtering
- Use NuGet packages `AspNetCore.HealthChecks.*` for common dependencies
- Use the `nuget-manager` skill for adding health check packages

## CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("https://app.example.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

app.UseCors("AllowFrontend");
```

## Rate Limiting (.NET 7+)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

app.UseRateLimiter();
```

- Apply per-endpoint with `[EnableRateLimiting("api")]` or `.RequireRateLimiting("api")`

## Output Caching (.NET 7+)

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("NoCache", builder => builder.NoCache());
});
```

## Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
```
