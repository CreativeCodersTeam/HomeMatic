---
name: convert-plaintext-to-md
description: Converts plain text or legacy documentation files to properly formatted Markdown. Use when asked to convert, migrate, or reformat a plaintext file to .md format. Supports reference-guided conversion, formatting options (headers, code blocks, indentation), and finalization/cleanup of existing .md files.
---

# Convert Plaintext Documentation to Markdown

## Conversion Methods

You can perform conversions using one of three approaches:

1. **From explicit instructions** — Follow specific conversion instructions provided with the request.
2. **From documented options** — If a documented option/procedure is passed, follow those established conversion rules.
3. **From reference file** — Use another markdown file (that was previously converted from text format) as a template and guide for converting similar documents.

When using a reference file, apply the same formatting patterns, structure, and conventions — while adapting to the specific content of the file being converted.

## Workflow

1. Determine the input file and check whether a `.md` counterpart already exists (see [parameters.md](./references/parameters.md))
2. Identify which conversion method applies
3. Apply the requested parameters and options (see [parameters.md](./references/parameters.md))
4. Follow pattern recognition and content preservation rules (see [considerations.md](./references/considerations.md))
5. If `finalize` is passed, clean up formatting after conversion

## References

- [Parameters and Options](./references/parameters.md) — Full parameter reference, options (`--header`, `--pattern`, `--stop`), predefined instructions
- [Usage Examples](./references/examples.md) — Worked examples: basic conversion, guide-based, finalize, inline regex pattern
- [Conversion Considerations](./references/considerations.md) — Pattern recognition, conversion goals, markdown style references

> [!IMPORTANT]
> Do not change the data content unless the prompt instructions clearly and without a doubt specify to do so.
