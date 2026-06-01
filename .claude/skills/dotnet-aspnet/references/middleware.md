# Middleware Pipeline

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

## Custom Middleware

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
