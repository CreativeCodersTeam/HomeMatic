# Dependency Injection

## Registration

Register services on the host's `IServiceCollection`. The same API applies to ASP.NET Core (`builder.Services`), Worker Services, Console apps using `HostApplicationBuilder`, and library extension methods (`public static IServiceCollection AddMyFeature(this IServiceCollection services)`).

```csharp
services.AddScoped<IOrderService, OrderService>();
services.AddSingleton<IClock, SystemClock>();
services.AddTransient<IEmailFormatter, EmailFormatter>();
```

- Register services in `Program.cs` or via extension methods (`AddApplicationServices()`, `AddPersistence()`, etc.) to keep `Program.cs` lean.
- Prefer interface-based registration for testability — register against the abstraction, not the concrete type.

## Lifetimes

- **Transient** — Stateless, lightweight services. New instance per resolution.
- **Scoped** — One instance per logical operation (per HTTP request in ASP.NET Core, per `IServiceScope` in worker scenarios). Typical for `DbContext`, unit-of-work, and per-request caches.
- **Singleton** — Thread-safe shared state (caches, configuration providers, expensive-to-build clients). Must be safe for concurrent use.

Lifetime mismatches (e.g. a Singleton capturing a Scoped service) are a common bug — `ValidateScopes` and `ValidateOnBuild` catch this at startup.

```csharp
var host = Host.CreateApplicationBuilder()
    .ConfigureContainer<IServiceCollection>((ctx, services) =>
    {
        // Register here
    })
    .Build();
```

For ASP.NET Core, scope validation is on by default in Development. For other hosts, enable it explicitly via `ServiceProviderOptions`.

## Keyed Services (.NET 8+)

Use keyed services when multiple implementations of the same interface need to coexist and be selected by key:

```csharp
services.AddKeyedScoped<IPaymentGateway, StripeGateway>("stripe");
services.AddKeyedScoped<IPaymentGateway, PayPalGateway>("paypal");

public class CheckoutHandler(
    [FromKeyedServices("stripe")] IPaymentGateway stripe,
    [FromKeyedServices("paypal")] IPaymentGateway paypal)
{
    // ...
}
```

## Anti-Patterns

- **Service locator** — Do not inject `IServiceProvider` into business logic and resolve dependencies at runtime. Constructor-inject the specific dependencies you need.
- **Capturing scoped services in singletons** — A singleton holding a reference to a scoped service leaks the scoped instance and causes use-after-dispose bugs. Use `IServiceScopeFactory` to create a fresh scope when needed.
- **Concrete-type registration when an interface exists** — Registering `services.AddScoped<OrderService>()` instead of `services.AddScoped<IOrderService, OrderService>()` defeats substitution and testing.
- **Static state / singletons outside DI** — All cross-cutting state goes through DI. No `public static` mutable state.

## Library Extension Method Pattern

When shipping a NuGet library that registers services for consumers, expose a single extension method:

```csharp
public static class MyFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddMyFeature(
        this IServiceCollection services,
        Action<MyFeatureOptions>? configure = null)
    {
        services.AddOptions<MyFeatureOptions>()
            .BindConfiguration(MyFeatureOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (configure is not null)
            services.Configure(configure);

        services.AddScoped<IMyFeatureService, MyFeatureService>();
        return services;
    }
}
```
