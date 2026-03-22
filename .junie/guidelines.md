This is a general set of instructions that will be used were applicable.

# General Instructions

- Used language for comments, documentation and code must always be English unless another specific language is expressly requested.
- Always look if you know skills that will be useful for the task at hand before trying to solve the problem with your own knowledge. If you know skills that can be useful, ask if you should use them.
- Always ask for help if you are stuck.
- If a skill was explicitly requested in the prompt, use it without asking. If you can't find the skill, always ask if you should proceed without it.

## Code Quality
- Write clean, maintainable, and well-documented code
- Follow SOLID principles
- Use meaningful variable and function names
- Keep functions small and focused on a single responsibility

## Testing
- Write unit tests for all new functionality
- Aim for high test coverage
- Use meaningful test names that describe the test scenario

## Documentation
- Document public APIs and complex logic
- Keep documentation up-to-date with code changes
- Use clear and concise language

## Error Handling
- Always handle errors gracefully
- Provide meaningful error messages
- Log errors appropriately for debugging

## Security
- Never commit secrets or sensitive data
- Validate all user inputs
- Follow security best practices for the specific technology stack

-----------------------------------------------------------


GitHub Copilot must ignore the following content in this file, cause Copilot gets this infos from the files in the .github/instructions directory:

-----------------------------------------------------------


---
description: 'Guidelines for building C# applications'
applyTo: '**/*.cs'
---

# C# Development

## C# Instructions

- Always use the latest stable C# version available in the project's target framework.
- Write clear and concise comments for each function.

## General Instructions

- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices, including comments on why certain design decisions were made.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, mention their usage and purpose in comments.
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

## Naming Conventions

- Follow PascalCase for component names, method names and public members.
- Use camelCase for local variables.
- Use _camelCase for private fields.
- Prefix interface names with "I" (e.g., IUserService).
- Use naming conventions from surrounding code if different from Naming conventions above.

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Prefer file-scoped namespace declarations and single-line using directives.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`,
  `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Use the `csharp-docs` skill to ensure XML documentation follows best practices.
- Use `[UsedImplicitly]` from JetBrains.Annotations when types are only used via DI or reflection

## Project Setup and Structure

- Guide users through creating a new .NET project with the appropriate templates.
- Explain the purpose of each generated file and folder to build understanding of the project structure.
- Demonstrate how to organize code using feature folders or domain-driven design principles.
- Show proper separation of concerns with models, services, and data access layers.
- Explain the Program.cs and configuration system in ASP.NET Core including environment-specific settings.

## Modern C# Features

- Prefer **switch expressions** over switch statements
- Use **pattern matching** wherever possible
- Use **primary constructors** when no constructor body is needed

## Async/Await

- In **library code** always use `.ConfigureAwait(false)`
- In **tests** do not use `.ConfigureAwait(false)` (disabled via tests/.editorconfig)

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
- Do not add redundant null checks when the type system already guarantees non-null

## Data Access Patterns

- Guide the implementation of a data access layer using Entity Framework Core.
- Explain different options (SQL Server, SQLite, In-Memory) for development and production.
- Demonstrate repository pattern implementation and when it's beneficial.
- Show how to implement database migrations and data seeding.
- Explain efficient query patterns to avoid common performance issues.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.

## Validation and Error Handling

- Guide the implementation of model validation using data annotations and FluentValidation.
- Explain the validation pipeline and how to customize validation responses.
- Demonstrate a global exception handling strategy using middleware.
- Show how to create consistent error responses across the API.
- Explain problem details (RFC 7807) implementation for standardized error responses.

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Use the `tester` skill for detailed testing conventions and workflows.

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.

## Deployment and DevOps

- Guide users through containerizing their API using .NET's built-in container support (
  `dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`).
- Explain the differences between manual Dockerfile creation and .NET's container publishing features.
- Explain CI/CD pipelines for NET applications.
- Demonstrate deployment to Azure App Service, Azure Container Apps, or other hosting options.
- Show how to implement health checks and readiness probes.
- Explain environment-specific configurations for different deployment stages.

-----------------------------------------------------------


