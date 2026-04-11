---
name: dotnet-aspnet
description: ASP.NET Core best practices for building Web and REST APIs. Use when creating controllers, minimal APIs, configuring middleware, routing, model binding, validation, dependency injection, authentication, authorization, error handling with ProblemDetails, OpenAPI/Swagger, health checks, CORS, rate limiting, or structuring an ASP.NET Core project.
---

# ASP.NET Core Web API Best Practices

## Project Structure

- Organize code by feature/domain, not by layer (avoid generic `Controllers/`, `Services/`, `Models/` folders)
- Use `Program.cs` as the composition root — register services, configure middleware pipeline
- Keep `Program.cs` lean by extracting registration into extension methods (e.g., `AddApplicationServices()`, `AddAuthenticationServices()`)
- Use feature folders:
  ```
  Features/
    Orders/
      OrdersController.cs
      CreateOrderRequest.cs
      OrderResponse.cs
      OrderService.cs
    Products/
      ...
  ```

## Controllers vs Minimal APIs

### Controllers

Use controllers for larger APIs with shared conventions, filters, and complex routing:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await orderService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }
}
```

- Always use `[ApiController]` attribute — enables automatic model validation, binding source inference, and ProblemDetails responses
- Use primary constructor injection for dependencies
- Return `IActionResult` or `ActionResult<T>` for endpoints with multiple response types
- Accept `CancellationToken` on all async endpoints

### Minimal APIs

Use minimal APIs for simple endpoints, microservices, or when startup performance matters:

```csharp
var group = app.MapGroup("/api/orders")
    .WithTags("Orders")
    .RequireAuthorization();

group.MapGet("/{id:guid}", async (Guid id, IOrderService service, CancellationToken ct) =>
{
    var order = await service.GetByIdAsync(id, ct);
    return order is null ? Results.NotFound() : Results.Ok(order);
})
.WithName("GetOrderById")
.Produces<OrderResponse>()
.ProducesProblem(StatusCodes.Status404NotFound);
```

- Use `MapGroup()` to share route prefixes, filters, and metadata
- Use `TypedResults` instead of `Results` for compile-time response type verification
- Add `.WithName()` for OpenAPI operation IDs and link generation
- Use endpoint filters for cross-cutting concerns

## Routing & Endpoints

- Use attribute routing on controllers (`[Route]`, `[HttpGet]`, etc.)
- Apply route constraints: `{id:guid}`, `{page:int:min(1)}`, `{slug:regex(^[a-z-]+$)}`
- Use API versioning via URL segment (`/api/v1/orders`) or header-based versioning with `Asp.Versioning.Http`
- Keep route templates consistent — plural nouns for resources (`/api/orders`, not `/api/order`)
- Use `LinkGenerator` for generating URLs to named endpoints

## Model Binding & Validation

- Use binding source attributes explicitly: `[FromBody]`, `[FromQuery]`, `[FromRoute]`, `[FromHeader]`
- With `[ApiController]`, complex types default to `[FromBody]`, simple types to `[FromRoute]`/`[FromQuery]`
- Validate with Data Annotations or FluentValidation:

```csharp
public record CreateOrderRequest(
    [Required] string CustomerId,
    [Required, MinLength(1)] List<OrderItemRequest> Items);
```

- For FluentValidation: register validators via `AddValidatorsFromAssemblyContaining<T>()` and use a validation filter or `IEndpointFilter`
- Return `ValidationProblemDetails` (automatic with `[ApiController]`) for validation failures

## Dependency Injection

- Register services in `Program.cs` or via extension methods
- Use appropriate lifetimes:
  - **Transient**: Stateless, lightweight services
  - **Scoped**: Per-request services (DbContext, unit of work)
  - **Singleton**: Thread-safe, shared state (caches, configuration)
- Use keyed services (.NET 8+) when multiple implementations of the same interface are needed:
  ```csharp
  builder.Services.AddKeyedScoped<IPaymentGateway, StripeGateway>("stripe");
  builder.Services.AddKeyedScoped<IPaymentGateway, PayPalGateway>("paypal");
  ```
- Prefer interface-based registration for testability
- Avoid service locator pattern — do not inject `IServiceProvider` into business logic

## Middleware Pipeline

Order matters — register middleware in the correct sequence:

```csharp
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHsts();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();
app.MapControllers();
```

### Custom Middleware

```csharp
public class RequestTimingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Response-Time-Ms"] = sw.ElapsedMilliseconds.ToString();
            return Task.CompletedTask;
        });
        await next(context);
    }
}
```

- Use primary constructors for middleware
- Inject scoped services via `InvokeAsync` parameters, not the constructor
- Keep middleware focused — one concern per middleware

## Configuration

- Use the Options pattern for strongly-typed configuration:

```csharp
public class SmtpOptions
{
    public const string SectionName = "Smtp";
    public required string Host { get; init; }
    public int Port { get; init; } = 587;
    public required string Username { get; init; }
}

builder.Services.AddOptions<SmtpOptions>()
    .BindConfiguration(SmtpOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

- Use `ValidateOnStart()` to catch configuration errors at startup, not at first use
- Use `appsettings.json` for defaults, `appsettings.{Environment}.json` for overrides
- Use User Secrets (`dotnet user-secrets`) for local development — never commit secrets
- Use environment variables or a vault for production secrets
- Inject `IOptions<T>` for static config, `IOptionsMonitor<T>` for reloadable config

## Authentication & Authorization

### JWT Bearer Authentication

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });
```

### Authorization Policies

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("CanEditOrder", policy =>
        policy.Requirements.Add(new OrderOwnerRequirement()));
```

- Use policy-based authorization over role checks in attributes
- Apply `[Authorize]` at controller level, `[AllowAnonymous]` for exceptions
- Implement `IAuthorizationHandler` for resource-based authorization
- For minimal APIs use `.RequireAuthorization("PolicyName")`

## Error Handling

### ProblemDetails (RFC 9457)

```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});
```

### Global Exception Handling (.NET 8+)

```csharp
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred",
            Type = "https://httpstatuses.com/500"
        };

        context.Response.StatusCode = problem.Status.Value;
        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

- Use `IExceptionHandler` (.NET 8+) instead of custom exception middleware
- Map domain exceptions to appropriate HTTP status codes
- Never expose internal exception details in production responses
- Use `app.UseStatusCodePages()` for consistent responses on empty status codes (404, 405, etc.)

## API Conventions & Response Types

- Annotate endpoints with `[ProducesResponseType]` for OpenAPI documentation
- Use `[Consumes]` and `[Produces]` for content type negotiation
- Return consistent response envelopes or use ProblemDetails for errors
- Use `TypedResults` in minimal APIs for compile-time response verification:

```csharp
group.MapPost("/", async (CreateOrderRequest req, IOrderService service, CancellationToken ct) =>
{
    var id = await service.CreateAsync(req, ct);
    return TypedResults.Created($"/api/orders/{id}", new { id });
});
```

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
- Use the `csharp-docs` skill for writing XML documentation comments

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

## Cross-Cutting Concerns

### CORS

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

### Rate Limiting (.NET 7+)

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

### Output Caching (.NET 7+)

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("NoCache", builder => builder.NoCache());
});
```

### Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
```

## Related Skills

- **[ef-core](../ef-core/SKILL.md)** — Data access with Entity Framework Core
- **[dotnet-tester](../dotnet-tester/SKILL.md)** — Unit and integration testing
- **[csharp-docs](../csharp-docs/SKILL.md)** — XML documentation comments
- **[nuget-manager](../nuget-manager/SKILL.md)** — NuGet package management
- **[dotnet-sdk-builder](../dotnet-sdk-builder/SKILL.md)** — SDK/client library generation
