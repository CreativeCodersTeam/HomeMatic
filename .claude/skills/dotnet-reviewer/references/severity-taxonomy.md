# Severity Taxonomy and Area Tags

Every finding is tagged `[Severity][Area]` followed by `path:line`.

## Severity Levels

| Severity | When to use | Examples |
|---|---|---|
| **Critical** | Ship-blocker. Production correctness, security, data loss, or a failing test. | SQL injection, unhandled `null` deref on hot path, failing test, build error. |
| **Major** | Will hurt users or maintainers but not a ship-blocker. | Missing input validation on a public API, race condition under load, broken contract with caller. |
| **Minor** | Real issue, low impact, deserves a fix in this PR. | Build warning, sloppy exception handling, missing log context, dead code. |
| **Suggestion** | Improvement worth considering. Author can accept or reject. | Refactor opportunity, alternative idiom, better naming, format violation. |
| **Nitpick** | Cosmetic. Author should ignore unless trivial. | Whitespace, comment phrasing, minor style preference. |

## Area Tags

Pick the **dominant** concern. If two apply, pick the higher-severity area.

| Tag | Scope |
|---|---|
| `Security` | Authn/authz, input validation, secrets, injection, deserialization, crypto, OWASP. |
| `Performance` | Allocations, async/await misuse, EF query shape, hot-path heuristics, Span/Memory. |
| `Architecture` | Layer/dependency direction, SOLID, DI misuse, pattern consistency, coupling. |
| `Code-Quality` | Naming, complexity, nullability, IDisposable, dead code, exception strategy. |
| `Tests` | Missing coverage, flaky tests, weak assertions, test maintenance smell. |
| `.NET-Idioms` | Version-specific idioms (Primary Ctors, Collection Expressions, Required Members, …). |

## Mapping from Tool Outputs

| Tool finding | Severity | Area |
|---|---|---|
| `dotnet build` error | Critical | Code-Quality (or context-driven) |
| `dotnet build` warning | Minor | Code-Quality |
| `dotnet test` failure | Critical | Tests |
| `dotnet format` violation | Suggestion | Code-Quality |

## Examples

```
[Critical][Security] src/Api/UserController.cs:42
User input is concatenated directly into a SQL string.

Use parameterized queries via `FromSqlInterpolated` or EF Core's LINQ.

```csharp
// before
var users = ctx.Users.FromSqlRaw($"SELECT * FROM Users WHERE Name = '{name}'");

// after
var users = ctx.Users.FromSqlInterpolated($"SELECT * FROM Users WHERE Name = {name}");
```
```
