# New Project Setup for .NET SDK Libraries

## When to Create a New Project

Create a new class library project when:
- No suitable existing project is found in the solution.
- The user explicitly requests a new project.
- Adding to an existing project would create an inappropriate coupling (e.g., adding an HTTP client library to a web API project).

Always ask the user for confirmation before creating a new project if not explicitly instructed.

## Project File Template

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Http" Version="*" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="*" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="*" />
  </ItemGroup>

</Project>
```

**Key settings:**
- `<Nullable>enable</Nullable>` — always for new projects when .NET version supports it.
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — required for XML doc generation.
- `<NoWarn>CS1591</NoWarn>` — suppress "missing XML comment" warnings during development; remove after `dotnet-xmldocs` skill adds all docs.
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` — enforces code quality.
- `<LangVersion>latest</LangVersion>` — enables the latest C# features for the target framework.

Replace `net9.0` with the .NET version determined in Step 2 of the workflow.

## Folder Structure

```
MyCompany.GitHub/
├── MyCompany.GitHub.csproj
├── GitHubServiceCollectionExtensions.cs   ← DI registration
├── GitHubOptions.cs                        ← Options class
├── IGitHubClient.cs                        ← Public interface
├── GitHubClient.cs                         ← Internal implementation
├── Exceptions/
│   ├── GitHubException.cs                  ← Base exception
│   ├── GitHubAuthenticationException.cs
│   ├── GitHubNotFoundException.cs
│   └── GitHubRateLimitException.cs
└── Models/
    ├── Repository.cs
    ├── User.cs
    └── ...
```

**Rules:**
- Root of the project: extension method, options, interface, implementation.
- `Exceptions/` folder: all exception types.
- `Models/` folder: all DTOs (request/response models).
- No `Services/` or `Abstractions/` sub-folders for small libraries — keep it flat.
- For larger libraries with multiple API areas, group by area: `Repositories/`, `Users/`, etc.

> This layout applies to **library** projects. Web/API applications should organize by feature/domain instead — see the `dotnet-aspnet` skill.

## Naming Conventions

| Artifact | Pattern | Example |
|---|---|---|
| Project | `Company.ServiceName` | `Acme.Stripe` |
| Namespace | Match project name | `Acme.Stripe` |
| Models namespace | `Company.ServiceName.Models` | `Acme.Stripe.Models` |
| Interface | `IXxxClient` | `IStripeClient` |
| Implementation | `XxxClient` | `StripeClient` |
| Options | `XxxOptions` | `StripeOptions` |
| Extension class | `XxxServiceCollectionExtensions` | `StripeServiceCollectionExtensions` |
| Extension method | `AddXxx` | `AddStripe` |
| Base exception | `XxxException` | `StripeException` |

## Adding to Solution

After creating the `.csproj`, add it to the solution:

```bash
dotnet sln add path/to/MyCompany.GitHub/MyCompany.GitHub.csproj
```

## Nullable in Existing Projects

When adding files to an existing project that has `<Nullable>disable</Nullable>` or no nullable setting:

Add at the top of each new source file:

```csharp
#nullable enable
```

Do **not** change the `.csproj` setting — this would affect all existing files in the project.

## Global Usings (Optional)

For .NET 6+, a `GlobalUsings.cs` file reduces boilerplate in every source file:

```csharp
global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Json;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Http;
global using Microsoft.Extensions.Options;
```

Create this file only when the project has more than ~5 source files that all use these namespaces.
