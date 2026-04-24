---
description: 'Guidelines for building C# applications'
applyTo: '**/*.cs'
---

# C# Development

## C# Instructions

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

- Use **primary constructors** when no constructor body is needed.
- Use private fields with guards instead of using primary constructor parameters directly, unless the parameter is assigned to a property.

## Async/Await

- In **library code** always use `.ConfigureAwait(false)`
- In **tests** do not use `.ConfigureAwait(false)` (disable for tests via tests/.editorconfig)

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations — don't add null checks when the type system guarantees non-null.

## Documentation

- Document all public members with XML documentation.
- Use the `csharp-docs` skill to ensure XML documentation follows best practices.
- If you change code, always update the relevant XML documentation.

## Testing

- Always include test cases for critical paths of the application.
- Always use the `dotnet-tester` skill for detailed testing conventions and workflows when writing tests.

## Console

- Use AnsiConsole for console input and output. Always use IAnsiConsole via dependency injection.
- Use colored output where it makes sense. For example, use green for success messages, red for errors and yellow for warnings.
- Use tables for structured output when displaying lists of data or multiple pieces of related information.

## Logging

- Use Serilog for logging.
- Configure Serilog with appropriate sinks (e.g., file, console, Azure Application Insights) based on environment.
- Always use structured logging with properties for better log analysis and correlation.

## Skills Reference

- Use the `dotnet-aspnet` skill for ASP.NET Core projects (project structure, middleware, auth, validation, error handling, API versioning, OpenAPI).
- Use the `ef-core` skill for Entity Framework Core data access patterns.
- Use the `dotnet-sdk-builder` skill for creating .NET SDK/client libraries.
- Use the `nuget-manager` skill for NuGet package management.
