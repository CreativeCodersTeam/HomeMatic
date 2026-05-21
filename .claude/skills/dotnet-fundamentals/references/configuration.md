# Configuration

Configuration in .NET is layered — later sources override earlier ones. The default `HostApplicationBuilder` order is:

1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g. `appsettings.Development.json`)
3. User Secrets (Development only)
4. Environment variables
5. Command-line arguments

The first source that supplies a key wins for that key; sources don't merge sections deeply, they override per leaf value.

## `appsettings.json`

Use for defaults that ship with the application:

```json
{
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

Use `appsettings.{Environment}.json` for environment-specific overrides:

```json
// appsettings.Development.json
{
  "Smtp": { "Host": "localhost" }
}
```

The active environment is set via `DOTNET_ENVIRONMENT` (generic host) or `ASPNETCORE_ENVIRONMENT` (ASP.NET Core).

## User Secrets

For local development secrets, never commit them. Initialize once per project:

```bash
dotnet user-secrets init
dotnet user-secrets set "Smtp:Password" "dev-password"
```

User Secrets are stored outside the project directory (in `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`) and are only loaded in the Development environment.

## Environment Variables

Use for production secrets and container/Kubernetes deployments. Nested keys use `__` (double underscore) as separator:

```bash
export Smtp__Host=smtp.prod.example.com
export Smtp__Password=$(cat /run/secrets/smtp-password)
```

The `__` separator is portable (works on Linux, macOS, Windows). `:` is also accepted but is not valid in environment variable names on Linux.

## Production Secret Stores

For production, integrate a real secret store rather than environment variables when secrets need rotation, auditing, or RBAC:

- **Azure Key Vault** — `AddAzureKeyVault(...)` from `Azure.Extensions.AspNetCore.Configuration.Secrets`
- **AWS Secrets Manager** — via `AWSSDK.Extensions.NETCore.Setup`
- **HashiCorp Vault** — via `VaultSharp` plus a custom configuration provider

These plug into the same `IConfiguration` pipeline, so consuming code keeps using `IOptions<T>` — only the registration in `Program.cs` differs.

## Anti-Patterns

- **Hard-coded secrets** — never commit a real password, API key, or connection string to source control. Local defaults should point at obviously-fake values.
- **`IConfiguration` injected into business logic** — bind to a typed Options class instead. Business logic should never know about configuration providers.
- **`config["Foo:Bar"]` indexing across the codebase** — magic strings + no validation. Bind once, inject `IOptions<T>`.
- **Per-environment code branching** — if behavior differs between Development and Production, model it as configuration, not as `if (env.IsDevelopment())` branches scattered through services.
