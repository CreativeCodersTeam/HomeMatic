---
name: dotnet
description: Entry point and router for .NET and C# work — directs you to the right specialized .NET skill. Use when a request mentions .NET or C# in general but the specific tool is not obvious, or to get an overview of the available .NET skills. Routes to knowledge skills (dotnet-fundamentals, dotnet-aspnet, ef-core, dotnet-xmldocs) and workflow skills (dotnet-sdk-builder, dotnet-tester, dotnet-reviewer, dotnet-inspect, nuget-manager). When the matching skill is already clear, invoke that skill directly instead.
---

# .NET / C# Skill Router

Signpost to the specialized .NET skills. This skill holds no best-practice knowledge
of its own — it maps a request to the appropriate specialized skill.

## When to Use

- A task concerns .NET or C#, but it is unclear which specialized skill applies.
- You need an overview of the available .NET skills and their responsibilities.

When the matching skill is already clear, load it directly — the router is only
orientation, not an intermediate step.

## Routing

### Knowledge skills (best practices, `references/` only)

| Concern | Skill |
|---------|-------|
| Dependency Injection, Options pattern, Configuration, modern C# idioms (for any .NET host) | `dotnet-fundamentals` |
| ASP.NET Core Web/REST APIs: controllers, Minimal APIs, middleware, routing, model binding, validation, auth, ProblemDetails, OpenAPI, health checks, CORS, rate limiting | `dotnet-aspnet` |
| Entity Framework Core: DbContext, entities/relationships, LINQ, migrations, repository patterns, N+1/performance | `ef-core` |
| C# XML doc comments (`<summary>`, `<param>`, `<returns>`, …) | `dotnet-xmldocs` |

### Workflow skills (active tools, scripts, agents)

| Concern | Skill |
|---------|-------|
| Generate a .NET SDK / client library / typed HTTP client | `dotnet-sdk-builder` |
| Write/run unit tests (xUnit, FakeItEasy, AwesomeAssertions) | `dotnet-tester` |
| Structured code review for .NET 10+ (explicit invocation only, see below) | `dotnet-reviewer` |
| Query .NET APIs in NuGet packages, platform libraries, or local files | `dotnet-inspect` |
| Manage NuGet packages (add/remove/update, `--outdated`, Central Package Management) | `nuget-manager` |

## Notes

- **`dotnet-reviewer` activates only on explicit name** — the phrases
  `dotnet-reviewer`, `dotnet code review`, or `dotnet review`. It does **not** trigger
  on generic "review my code", and the router does not trigger it automatically.
- **Composition:** `dotnet-sdk-builder` invokes `dotnet-xmldocs` and `dotnet-tester`;
  `dotnet-aspnet` and `ef-core` build on `dotnet-fundamentals`.
