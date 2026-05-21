---
name: dotnet-fundamentals
description: Applies modern .NET fundamentals — dependency injection, Options pattern, configuration, and modern C# idioms. Use when registering services in any .NET host (ASP.NET Core, Worker Service, Console, MAUI), binding configuration with IOptions<T>, choosing DI lifetimes, configuring appsettings.json / User Secrets / environment variables, or applying primary constructors, required properties, nullable reference types, and CancellationToken propagation.
---

# Modern .NET Fundamentals

## When to Use

- Registering services in any `IServiceCollection` (ASP.NET Core, Worker Service, Console app, MAUI, library DI extension methods)
- Choosing a DI lifetime (Transient, Scoped, Singleton) or registering keyed services (.NET 8+)
- Binding configuration sections to a strongly-typed Options class
- Setting up `appsettings.json`, environment-specific overrides, User Secrets, or environment variables
- Adopting primary constructors, `required` properties, nullable reference types, or `CancellationToken` propagation in new code

This skill is **technology-agnostic across .NET hosts**. ASP.NET Core, EF Core, and SDK builders all sit on top of these fundamentals.

## Core Principles

- **Interface-first registration** — register services via their abstraction (`AddScoped<IFoo, Foo>()`), not the concrete type. Enables substitution and testing.
- **No service locator** — never inject `IServiceProvider` into business logic. Constructor-inject the dependencies you actually need.
- **Options over constructor parameters for configuration** — bind config sections to `IOptions<T>`, do not pass raw `IConfiguration` values around.
- **Fail fast** — use `ValidateDataAnnotations().ValidateOnStart()` so misconfiguration surfaces at startup, not at first use.
- **Immutable configuration** — Options classes use `required` properties and `init`-only setters.
- **Cancellation flows everywhere** — every async method takes a `CancellationToken` as its last parameter and forwards it.

## Reference Index

- **[dependency-injection.md](references/dependency-injection.md)** — `IServiceCollection` registration, lifetimes, keyed services, interface-based registration, anti-service-locator
- **[options-pattern.md](references/options-pattern.md)** — `IOptions<T>` vs `IOptionsMonitor<T>` vs `IOptionsSnapshot<T>`, `BindConfiguration`, `ValidateDataAnnotations`, `ValidateOnStart`
- **[configuration.md](references/configuration.md)** — `appsettings.json` and environment overrides, User Secrets, environment variables, production secret stores
- **[modern-patterns.md](references/modern-patterns.md)** — primary constructors, `required` / `init`-only properties, nullable reference types, `CancellationToken` propagation

## Related Skills

- **[dotnet-aspnet](../dotnet-aspnet/SKILL.md)** — Builds the HTTP layer on top of these fundamentals (middleware, controllers, minimal APIs)
- **[dotnet-sdk-builder](../dotnet-sdk-builder/SKILL.md)** — Emits SDK libraries that use these patterns (DI extension methods, typed Options, typed HTTP clients)
- **[ef-core](../ef-core/SKILL.md)** — Registers `DbContext` via DI, binds connection strings via Options
- **[dotnet-reviewer](../dotnet-reviewer/SKILL.md)** — Reviews code against these fundamentals during code review
- **[dotnet-tester](../dotnet-tester/SKILL.md)** — Tests DI-registered services using `FakeItEasy` and `ServiceCollection` overrides
- **[nuget-manager](../nuget-manager/SKILL.md)** — Adds `Microsoft.Extensions.*` packages required for DI, Options, and Configuration
