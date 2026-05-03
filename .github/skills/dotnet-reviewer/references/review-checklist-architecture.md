# Review Checklist — Architecture

## Layering

- Domain references no infrastructure (no EF Core attributes on domain entities; no `HttpClient` in domain).
- Application layer orchestrates — does not reach into UI/infrastructure types directly.
- Controllers / minimal-API handlers stay thin: parse, dispatch to application service, map result.
- Infrastructure adapters implement domain/application interfaces, not the other way around.

## Dependency Direction

- Outer rings depend on inner rings. Flag any inner-ring file with a `using` of an outer-ring namespace.
- Solution structure should make this provable; if it doesn't (single project, mixed namespaces), call it out.

## SOLID

- **SRP:** classes that change for multiple reasons. Public surface that mixes orchestration with persistence.
- **OCP:** strategy patterns where `if (type == X) … else if (type == Y) …` is repeated across files.
- **LSP:** subclass that throws `NotSupportedException` on a base member.
- **ISP:** "fat" interfaces with one consumer per method. Split or use mark-only interfaces.
- **DIP:** new-ing dependencies inside a class instead of injecting (except for value objects).

## Dependency Injection

- Lifetimes: avoid `Singleton` capturing `Scoped` (e.g., `DbContext` in a `Singleton` cache).
- Factory delegates over `IServiceProvider` parameters in constructors.
- No `BuildServiceProvider()` in `Configure`/`ConfigureServices` — that creates a second container.
- Validation of options on startup (`ValidateOnStart`).

## Pattern Consistency

- New code matches the existing project pattern. If this file uses CQRS handlers and you added a controller method that bypasses them, flag it.
- Naming consistency: `XxxService` vs. `XxxRepository` vs. `XxxHandler` — pick one and stay consistent within a bounded context.
- New pattern introductions (e.g., switching from MediatR to direct calls) need an architectural rationale; flag if introduced silently.

## Module Boundaries

- Each project (.csproj) has a clear purpose. Flag projects whose name and contents have drifted.
- Public API of a project is intentional — `internal` over `public` unless cross-project consumption is required.
- `InternalsVisibleTo` only for tests; flag if used to share types with non-test projects.
