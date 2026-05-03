# Review Checklist — Code Quality

## Naming

- Methods are verbs. Properties are nouns. Async methods end in `Async`.
- No Hungarian notation (`strName`, `iCount`).
- Booleans read as questions: `IsValid`, `HasItems`, `CanRetry`.
- Acronyms: `Url`, `Id`, `Api` (PascalCase per .NET conventions, not `URL`/`ID`/`API`).
- Type parameters: `T`, or `TKey`/`TValue` — descriptive when more than one.

## Nullability

- `<Nullable>enable</Nullable>` is on (project-level check).
- No `!` (null-forgiving operator) without a comment explaining why.
- No `#nullable disable` regions in new code.
- API surface honors nullability annotations — methods returning `T?` actually return null in some path.

## Complexity

- Methods over ~30 lines or with 4+ levels of nesting deserve a closer look.
- A `switch` with more than ~7 arms over the same value usually wants polymorphism or a lookup table.
- Boolean parameters often hide two methods — flag and suggest splitting.

## Exception Handling

- Catch the specific exception type, not `Exception`. `catch (Exception)` requires justification.
- Never swallow exceptions silently. Logging counts only if the log line carries enough context to diagnose.
- Don't use exceptions for control flow.
- Wrap third-party exceptions at the boundary; do not leak them into domain code.

## IDisposable / IAsyncDisposable

- `using` (or `using` declarations) for every `IDisposable` you create.
- `IDisposable` types as fields require the owning class to also be disposable.
- `IAsyncDisposable` consumed with `await using`, not `using`.

## Dead Code

- Unused `using` directives → format check should catch; flag if format check is off.
- Unreachable code (`return` followed by statements).
- Unused private members. Public unused members may be API for a consumer — leave alone unless clearly orphaned.

## Comments and Docs

- Public API has XML docs, especially for libraries.
- Comments explain *why*, not *what*. Flag comments that restate the code.
- TODOs without a ticket reference are a smell; flag.

## Tests (cross-cutting)

- New public behavior has at least one test.
- Tests assert behavior, not implementation (no over-mocking).
- Names describe the scenario: `Method_Condition_Expected`.
- Arrange/Act/Assert layout is visible.
- No conditional logic in test bodies (`if`/`for` inside tests).
- Edge cases: null, empty collection, boundary values.

## Logging

- Structured logging (named placeholders), not string concatenation.
- Log level matches consequence: `Error` for exceptions reaching the boundary, `Warning` for recoverable degraded paths, `Information` for state transitions, `Debug` for detail.
- No PII in logs (email, user names, full request bodies) without a redaction strategy.
