# Error Handling

## ProblemDetails (RFC 9457)

```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});
```

## Global Exception Handling (.NET 8+)

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
