# General Instructions

- Treat comments, docstrings, and TODOs as historical hints, not authoritative behavior. They survive refactors and go stale. Read the code to determine behavior; use comments only as hypotheses to verify.
- **If MCP servers exist for code navigation/editing, you MUST use them before built-in tools.**
- Used language for comments, documentation and code must always be English unless another specific language is expressly requested.
- Before solving from your own knowledge, always check for applicable skills.
- Use subagents as much as possible to avoid context pollution.
- ALWAYS verify that your changes are complete and work correctly. Use verification steps best suited for your changes.

# Git Commit Instructions
- You MUST not git commit files unless explicitly asked to do so by the user.
- Stage files by name (never git add -A/.). Refuse to stage secret-like files (.env, credentials.json, *.pem); warn if the user insists.

# Coding Guidelines

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them — don't pick silently.
- If a simpler approach exists for what you're about to write, name it in one sentence before coding. If the user confirms the original, proceed.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios. No error handling for scenarios guaranteed impossible by the type system or a same-file invariant. If justifying the skip requires reasoning about callers, keep the check.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it in your final response — don't delete it.

When your changes create orphans: Remove imports/variables/functions your changes orphaned; leave pre-existing dead code (mention it in the response).

## Priority when rules conflict
1. Ask beats guessing or silent assumption.
2. Surgical beats Simplify for existing code. § 2 applies only to code you write new in this task; don't rewrite existing code to make it simpler unless asked.
3. Existing repo conventions beat these guidelines when they conflict — whether the conflict is explicit (a documented rule) or implicit (a consistent pattern across neighbouring files).

-----------------------------------------------------------


---
description: 'Guidelines for building C# applications'
applyTo: '**/*.cs'
---

# C# Development

- Always use the latest stable C# version available in the project's target framework.

## General Instructions

- Use `Ensure.NotNull(...)` from `CreativeCoders.Core` for null guards
- Use `Ensure.IsNotNullOrEmpty(...)` from `CreativeCoders.Core` for string guards for arguments that must not be empty
- Use `Ensure.IsNotNullOrWhitespace(...)` from `CreativeCoders.Core` for string guards for arguments that must not be empty or whitespace
- Guard arguments for public methods in libraries with `Ensure.NotNull(...)` for all required parameters:
```csharp
public void DoSomething(string input, string fileName)
{
    Ensure.NotNull(input);
    Ensure.NotNullOrWhitespace(fileName);
    // method implementation
}
```
- Guard constructor-injected dependencies with `Ensure.NotNull(...)` for all required parameters:
```csharp
_service = Ensure.NotNull(service);
```

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Prefer file-scoped namespace declarations and single-line using directives.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`,
  `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use `nameof` instead of string literals when referring to member names.
- Use `[UsedImplicitly]` from JetBrains.Annotations when types are only used via DI or reflection.
- Use naming conventions from surrounding code when they differ from standard C# conventions.

## Modern C# Features

- **Default to a primary constructor**, also with `Ensure.*` guards — put the guard in the field initializer, not a constructor body:
  ```csharp
  public sealed class Foo(IBar bar) : IFoo
  {
      private readonly IBar _bar = Ensure.NotNull(bar);
  }
  ```
- Reference the fields, never the raw parameters (avoids capturing unguarded params).
- Use a classic constructor only when init needs real statements (control flow, ordering, multistep setup, logic before base(...)/this(...)). Guards/initializers don't count.
- A parameter assigned to a property goes via the property initializer, not a backing field.

## Async/Await

- In **library code** always use `.ConfigureAwait(false)`
- In **tests** do not use `.ConfigureAwait(false)` (disable for tests via tests/.editorconfig)
- YOU MUST NOT USE `.GetAwaiter().GetResult()` OR `.Result` OR `.Wait()` TO BLOCK ON ASYNC CODE. If there is no other way ask the user what to do.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations — don't add null checks when the type system guarantees non-null.

## Documentation

- Document all public members with XML documentation.
- Use the `dotnet-xmldocs` skill to ensure XML documentation follows best practices.
- If you change code, always update the relevant XML documentation.

## Testing

- Always include test cases for code changes.
- Always use the `dotnet-tester` skill for writing tests.

## Console

- Use AnsiConsole for console input and output. Always use IAnsiConsole via dependency injection.
- Use colored output where it makes sense. For example, use green for success messages, red for errors and yellow for warnings.
- Use tables for structured output when displaying lists of data or multiple pieces of related information.

## Logging

- Use Serilog for logging.
- Configure Serilog with appropriate sinks (e.g., file, console, Azure Application Insights) based on environment.
- Always use structured logging with properties for better log analysis and correlation.

## Skills Reference

- You MUST use the `dotnet-aspnet` skill for ASP.NET Core projects (project structure, middleware, auth, validation, error handling, API versioning, OpenAPI).
- You MUST use the `dotnet-ef-core` skill for Entity Framework Core data access patterns.
- You MUST use the `dotnet-sdk-builder` skill for creating .NET SDK/client libraries.
- You MUST use the `dotnet-reviewer` skill for Reviewing .NET Code.
- You MUST use the `dotnet-tester` skill for writing and editing tests.
- You MUST use the `dotnet-nuget-manager` skill for NuGet package management.
- You MUST use the `dotnet-inspect` skill to query .NET APIs in NuGet packages, platform libraries (System.*, Microsoft.AspNetCore.*), or local .dll/.nupkg files — discover types and members, diff API surfaces between versions, find extension methods/implementors, locate SourceLink URLs, and triage breakages caused by package upgrades.
- You MUST use the `dotnet-xmldocs` skill to ensure XML documentation follows best practices.

-----------------------------------------------------------


