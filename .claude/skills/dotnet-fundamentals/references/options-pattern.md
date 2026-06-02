# Options Pattern

Bind configuration sections to strongly-typed classes via `Microsoft.Extensions.Options`. Validate at startup, inject by interface (`IOptions<T>` / `IOptionsMonitor<T>` / `IOptionsSnapshot<T>`).

## Defining an Options Class

```csharp
public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public required string Host { get; init; }
    public int Port { get; init; } = 587;
    public required string Username { get; init; }
    public required string Password { get; init; }
}
```

- Declare `SectionName` as a `const string` on the class so consumers and tests can refer to it without magic strings.
- Use `required` properties for values without sane defaults — binding fails clearly if a required value is missing.
- Use `init`-only setters to keep options immutable after binding.

## Registration

```csharp
services.AddOptions<SmtpOptions>()
    .BindConfiguration(SmtpOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

- `BindConfiguration("Smtp")` binds the section automatically and re-binds when configuration reloads.
- `ValidateDataAnnotations()` runs `[Required]`, `[Range]`, etc. on the options class.
- `ValidateOnStart()` runs validation at host startup — misconfiguration surfaces immediately instead of at first use.

For complex validation, implement `IValidateOptions<T>`:

```csharp
public class SmtpOptionsValidator : IValidateOptions<SmtpOptions>
{
    public ValidateOptionsResult Validate(string? name, SmtpOptions options)
    {
        if (options.Port is < 1 or > 65535)
            return ValidateOptionsResult.Fail("Port must be 1-65535");
        return ValidateOptionsResult.Success;
    }
}

services.AddSingleton<IValidateOptions<SmtpOptions>, SmtpOptionsValidator>();
```

## Consuming Options

| Interface | Lifetime | When to use |
|---|---|---|
| `IOptions<T>` | Singleton | Static configuration that does not change after startup. Cheapest. |
| `IOptionsSnapshot<T>` | Scoped (ASP.NET Core only) | Per-request rebinding — picks up changes for each new request. |
| `IOptionsMonitor<T>` | Singleton with change notifications | Long-lived components (background services, HTTP clients) that need to react to reloads via `OnChange`. |

```csharp
public class EmailSender(IOptions<SmtpOptions> options)
{
    private readonly SmtpOptions _smtp = options.Value;
    // ...
}

public class ReloadableEmailSender(IOptionsMonitor<SmtpOptions> monitor)
{
    public ReloadableEmailSender(IOptionsMonitor<SmtpOptions> monitor)
    {
        monitor.OnChange(opts => RebuildClient(opts));
    }
}
```

## Named Options

When the same options shape exists multiple times (e.g. multiple SMTP backends):

```csharp
services.Configure<SmtpOptions>("Primary", config.GetSection("Smtp:Primary"));
services.Configure<SmtpOptions>("Fallback", config.GetSection("Smtp:Fallback"));

public class Mailer(IOptionsMonitor<SmtpOptions> monitor)
{
    private readonly SmtpOptions _primary = monitor.Get("Primary");
    private readonly SmtpOptions _fallback = monitor.Get("Fallback");
}
```
