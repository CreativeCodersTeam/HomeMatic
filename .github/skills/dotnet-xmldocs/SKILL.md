---
name: dotnet-xmldocs
description: Adds and reviews C# XML documentation comments following Microsoft's documentation standards. Use when writing or reviewing C# code that includes public APIs, complex logic, or when documentation is missing or insufficient. Covers <summary>, <param>, <returns>, <exception>, <remarks>, and all standard XML doc tags.
---

# C# Documentation Best Practices

## When to Use

- Writing or reviewing XML documentation comments (`///`) on C# types and members
- Adding documentation to public APIs of a new or existing C# library
- Reviewing missing, insufficient, or non-Microsoft-style XML docs in C# code
- Generating documentation that feeds OpenAPI/Swagger output or NuGet symbols

## General Guidelines

- Public members should be documented with XML comments.
- It is encouraged to document internal members as well, especially if they are complex or not self-explanatory.

## Guidance for all APIs

- Use `<summary>` to provide a brief, one sentence, description of what the type or member does. Start the summary with a present-tense, third-person verb.
- Use `<remarks>` for additional information, which can include implementation details, usage notes, or any other relevant context.
- Use `<see langword>` for language-specific keywords like `null`, `true`, `false`, `int`, `bool`, etc.
- Use `<c>` for inline code snippets.
- Use `<example>` for usage examples on how to use the member.
  - Use `<code>` for code blocks. `<code>` tags should be placed within an `<example>` tag. Add the language of the code example using the `language` attribute, for example, `<code language="csharp">`.
- Use `<see cref>` to reference other types or members inline (in a sentence).
- Use `<seealso>` for standalone (not in a sentence) references to other types or members in the "See also" section of the online docs.
- Use `<inheritdoc/>` to inherit documentation from base classes or interfaces.
  - Unless there is major behavior change, in which case you should document the differences.

## Member-Specific Rules

See [member-documentation-rules.md](./references/member-documentation-rules.md) for detailed wording conventions for methods (`<param>`, `<returns>`), constructors, properties (`<value>`, Gets/Sets patterns), and exceptions (`<exception cref>`).

## Related Skills

- **[dotnet-aspnet](../dotnet-aspnet/SKILL.md)** — XML docs feed OpenAPI/Swagger output
- **[dotnet-sdk-builder](../dotnet-sdk-builder/SKILL.md)** — Invokes this skill in Step 8 to document generated SDKs
- **[dotnet-reviewer](../dotnet-reviewer/SKILL.md)** — Code-quality checklist references these conventions for public-API docs
