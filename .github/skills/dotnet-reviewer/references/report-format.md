# Report Format

The reviewer writes one Markdown file to `docs/reviews/YYYY-MM-DD-<branch>-<mode>.md`.
Never overwrite — append `-2`, `-3`, … on collision. Never auto-commit.

## Required Sections

1. Title + metadata block
2. Executive Summary (detailed)
3. Findings, ordered by severity desc, then by file path
4. Tool Output Appendix (if any tool ran)

## Skeleton

```markdown
# .NET Code Review — <branch> (<mode>)

**Date:** YYYY-MM-DD
**Mode:** uncommitted | branch (vs. main)
**Detected SDK:** 10.0.x
**Target Framework(s):** net10.0, …
**Tools run:** build=Y/N · format=Y/N · test=Y/N
**Exclusions:** .gitignore, *.min.js, wwwroot/lib/**
**Review strategy:** full | prioritized | chunked
**Diff size:** <files> files, <loc> changed LOC

## Executive Summary

| Severity | Count |
|---|---|
| Critical | N |
| Major | N |
| Minor | N |
| Suggestion | N |
| Nitpick | N |

**Top risks:**
1. <one line>
2. <one line>
3. <one line>

**Overall:** <1–2 sentence assessment>

## Findings

### [Critical][Security] src/Api/UserController.cs:42
<one-line description>

<recommendation paragraph>

```csharp
// fix suggestion
```

### [Major][Performance] src/Data/Repo.cs:88
…

## Tool Output Appendix

### dotnet build
- 0 errors, 2 warnings (see findings above for warnings folded in).

### dotnet test
- 12 passed, 0 failed (duration: 4s).

### dotnet format
- Skipped.
```

## Rules

- **Every finding MUST include a fix suggestion** as a code block. If the fix is structural (no single-snippet rewrite), describe the steps in prose and provide the most-affected snippet.
- Do not paste raw diff content. Reference `path:line` instead.
- File paths are repo-relative.
- Group findings by severity desc, ties broken by file path asc.
- Tool warnings/errors that are already covered by a hand-written finding should not be duplicated in the appendix — note "folded into findings".
- If strategy `chunked` was used, group findings under `## Findings — <file>` subsections.
- If strategy `prioritized` was used, list deferred files at the end as `## Files Not Reviewed in Detail`.
