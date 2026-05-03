# Review Checklist — .NET 10

Apply when `detect-dotnet-version.sh` reports `target_frameworks` containing `net10.0`.

## Language Idioms

- **Primary constructors** — prefer over redundant private fields when the parameter is used directly. Flag: legacy `ctor + private readonly field` pattern in new code.
- **Collection expressions** — `[1, 2, 3]` over `new[] { 1, 2, 3 }` and `new List<int> { 1, 2, 3 }`. Flag: verbose collection initialization.
- **Required members** — `required` modifier replaces hand-rolled validation in constructors. Flag: throws in constructor for missing init-only properties.
- **`field` keyword** — auto-property backing-field access (preview in 9, stable in 10). Flag: unnecessary backing field declarations.
- **Pattern matching** — list patterns, relational patterns. Flag: chained `if (x.Length > 0 && x[0] == …)`.
- **`init` and `required` together** — for immutable POCOs.

## API Idioms

- `System.Threading.Lock` (new lock type) over `object`-based `lock` for new code.
- `Random.Shared` for non-cryptographic randomness — never `new Random()` in hot path.
- `TimeProvider` for testable time — flag direct `DateTime.UtcNow` in code that should be testable.
- `JsonSerializerContext` (source-gen) over reflection-based `JsonSerializer` on hot paths.

## Project Configuration

- `<Nullable>enable</Nullable>` MUST be on. Flag projects without it.
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` recommended for libraries.
- `<LangVersion>` should not be pinned below the SDK's default unless a comment explains why.
- `ImplicitUsings` enabled — flag stale top-of-file using directives that are already implicit.

## Things That Are Still Wrong

These are not new in .NET 10 but are still common:

- `.Result` / `.Wait()` on `Task` — sync-over-async deadlock risk.
- `async void` outside event handlers.
- `IEnumerable<T>` enumerated multiple times when the source is a generator.
- `string` concatenation in loops where `StringBuilder` or `string.Create` fits.
