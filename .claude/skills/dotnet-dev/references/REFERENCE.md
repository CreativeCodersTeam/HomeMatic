# dotnet-dev — Detailed Reference

In-depth guidance per phase. Assumes a .NET / C# project. The Skill Map and the
CRITICAL RULES in `SKILL.md` are authoritative; this document expands the *how*.

---

## Phase 1 — Requirement Review (Detail)

### Goal
Understand the requirement fully before any code is written, and surface every
gap before they become rework.

### Steps
1. **Read the requirement** (issue, story, ticket, message). Identify the core
   objective and expected outcome.
2. **Acceptance criteria** — explicit (stated) and implicit (existing tests must
   still pass, API contracts honored, public additions need XML docs, existing
   conventions respected).
3. **Gaps / contradictions / open questions** — write them down. Watch for:
   - Scope boundaries (what is in / out).
   - Error-handling behavior (`ProblemDetails`? typed exceptions? Result?).
   - Backward compatibility (API surface, EF Core migrations).
   - Target-framework / performance constraints.
   - Contradictions between the request and existing code/conventions.
4. **Codebase analysis** — affected `*.csproj`, host type (ASP.NET Core, Worker,
   Console, MAUI), data layer (EF Core), cross-cutting (DI, Options, Serilog).
   Locate existing tests (`*.Tests`) and docs. Read `Directory.Build.props`,
   `Directory.Packages.props`, `.editorconfig`, `nuget.config`. Use Serena /
   tokensave per project + global tooling rules — not `Explore` agents when
   tokensave is available.
5. **`dotnet-inspect`** — invoke whenever an external/platform/NuGet API surface
   must be verified (types, members, version diffs, extension methods).
6. **Preliminary Skill Map** — from the requirement, list which bindings the
   work will touch. It is *preliminary*: persistence (point 7) and dependencies
   (point 8) are confirmed in Phase 2 and may add `dotnet-ef-core` /
   `dotnet-nuget-manager`.

### GATE 1 output
Requirement restated in your words · acceptance criteria · gaps/contradictions/
open questions · affected scope · preliminary Skill Map table. **Wait.**

---

## Phase 2 — Clarification (Detail)

### Goal
Resolve the 8 standard implementation dimensions, one at a time, so nothing is
silently assumed.

### Protocol
- **One point per message.** Present point _N_: a concrete proposed default,
  then the open question. Wait for the answer before point _N+1_.
- **No batching, no skipping.** A point that objectively does not apply is shown
  with `n/a — <code-referenced reason>` and still acknowledged before moving on.
- Pre-filling a sensible default is encouraged — it lets the user reply "ok"
  fast — but the default never replaces the round-trip.

### The 8 points (expanded prompts)
1. **Project & folder structure** — which `*.csproj`, namespace, folder layout;
   new projects yes/no.
2. **Architecture & layering** — controller/service/repo vs vertical slice; DI
   lifetimes (singleton/scoped/transient); public vs internal surface.
3. **Naming conventions** — type/method/file/namespace names; suffixes
   (`Service`, `Handler`, `Options`, `Async`); test naming pattern.
4. **Public API / contracts** — DTO shape, request/response, `ProblemDetails`,
   versioning, OpenAPI annotations.
5. **Errors & edge cases** — exception strategy, Result vs throws, validation
   style, logging granularity.
6. **Test strategy** — unit and/or integration, mock boundaries, naming, fakes
   vs real dependencies, coverage expectation.
7. **Persistence / EF Core** — entity shape, migrations yes/no, owned types,
   indexes, query style (LINQ vs spec). Confirms `dotnet-ef-core`.
8. **Dependencies / NuGet** — allowed/forbidden packages, Central Package
   Management versions. Confirms `dotnet-nuget-manager`.

### GATE 2 output
All 8 clarified decisions · the **finalized** Skill Map. **Wait.**

---

## Phase 3 — Task Breakdown (Detail)

### Goal
A trackable plan of discrete tasks, each self-contained.

### Steps
- Break into the smallest reasonable tasks; descriptive IDs; enough detail to
  execute without re-reading the plan.
- Each task addresses **production code + tests + documentation**.
- Each task publishes its **Skill-prerequisite checklist** from the Skill Map.
- Dependencies — common .NET order: entities → EF Core config/migrations →
  repositories/services → controllers/endpoints; DTOs before consumers; package
  changes before code using them.
- Parallelization — group independent tasks for sub-agents; do not parallelize
  migration/schema work with other EF Core work.

### GATE 3 output
Task list with per-task checklists · dependency order · parallel groups. **Wait.**

---

## Phase 4 — Implementation + App Smoke-Check (Detail)

### Step 0 — Skill invocation (per task)
Before any `Write`/`Edit`/code-producing `Bash` for the task, invoke each
required binding once via the `Skill` tool. A task touching code + tests + docs
is at least three calls (`dotnet-fundamentals` + `dotnet-tester` +
`dotnet-xmldocs`) plus stack skills and `dotnet-nuget-manager` if packages
change. Wait for each skill's content; follow its workflow when producing the
artifact. Sub-agent dispatch is in addition to Step 0, not a substitute — pass
the skills explicitly to the sub-agent (sub-agents are stateless).

### Production code
`dotnet-fundamentals` always; plus `dotnet-aspnet` (web), `dotnet-ef-core` (EF),
`dotnet-sdk-builder` (typed SDK/HTTP client), `dotnet-inspect` (verify external
API). Apply project conventions: `Ensure.*` guards, primary constructors,
nullable reference types, `CancellationToken` propagation, `.ConfigureAwait(false)`
in library code (not in tests), never `.Result`/`.Wait()`/`.GetAwaiter().GetResult()`.

### Tests
`dotnet-tester` always when code is written/changed — xUnit + FakeItEasy +
AwesomeAssertions, plus its second-agent missing-case pass. Cover new/changed
behavior and the edge cases from Phase 1.

### Documentation
`dotnet-xmldocs` for every public API addition/change.

### Build & dependencies
`dotnet-nuget-manager` for any package add/remove/version change (enforces the
`dotnet` CLI and `Directory.Packages.props`). Then `dotnet build` + `dotnet test`;
fix failures before the gate.

### App Smoke-Check
- **Applies** when an executable project exists (`OutputType=Exe`, ASP.NET Core
  Web/API, Worker Service). **`n/a`** only when library-only.
- Start in the **background** (servers do not self-exit). Verify startup logs /
  health endpoint / one representative request. Shut down cleanly. Never block
  on a foreground long-running process.

### GATE 4 output
Implemented tasks · build/test result · App-Smoke-Check outcome (or `n/a`
reason). **Wait.**

---

## Phase 5 — Code Review (Detail)

### Steps
- Launch a code-review **sub-agent** instructed to invoke `dotnet-reviewer`;
  also invoke `dotnet-reviewer` in the main conversation. The skill reviews
  working-tree or branch-vs-`main` changes and writes a severity-tagged Markdown
  report under `docs/reviews/`.
- Review covers: correctness/completeness vs requirement, test coverage, doc
  accuracy, code quality/bugs/security, .NET idioms, convention consistency.

### Evaluate findings
- **No / minor issues** → fix directly (still invoking the required skills
  before edits), then gate.
- **Significant issues** → new tasks (each with its Skill checklist) → return to
  Phase 4 → re-run Phase 5. If the same issue recurs after 2 cycles, consult the
  user.

### GATE 5 output
Findings · report path · rework/no-rework decision. **Wait.**

---

## Phase 6 — Final Summary (Detail)

1. **Requirement** restated briefly.
2. **Files** created/modified.
3. **Implementation details** and key design decisions.
4. **Tests** added/updated and what they cover.
5. **Documentation** changes (XML docs, READMEs).
6. **Package changes** (cross-reference `Directory.Packages.props`).
7. **Decisions / trade-offs.**
8. **Review notes** — key `dotnet-reviewer` points + report link.
9. **Things to check** before committing.
10. **Skill-Invocation Log** — every task's checklist resolved to `[x]`/`[n/a]`/
    `[!]` with evidence (see `SKILL.md`). Mandatory even when all bindings were
    correct — it is the audit trail.

End with: *All changes are ready for your review. Commit them yourself when
satisfied — this workflow creates no commits.*

---

## General Guidelines

### Sub-agents
- Provide complete context (stateless). Pass the relevant `dotnet-*` skills in
  the prompt. Use a build/test runner for `dotnet build`/`dotnet test`. Prefer
  small focused tasks; launch independent tasks in parallel.
- For code research, follow the project + global tooling rules (Serena →
  tokensave → built-in; no `Explore` agents when tokensave is available).

### Git
- NEVER `git commit`, `git add -A`/`.`, branch, tag, or push. Read-only git
  (`diff`/`status`/`log`) is fine. Stage by name only when explicitly asked;
  refuse secret-like files (`.env`, `credentials.json`, `*.pem`).
- If a commit is requested, skip it: *"Committing is your responsibility."*

### Project conventions
- Honor `CLAUDE.md`, `AGENTS.md`, `.github/copilot-instructions.md`,
  `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`.
- `Ensure.NotNull(...)` / `Ensure.IsNotNullOrEmpty(...)` /
  `Ensure.IsNotNullOrWhitespace(...)` from `CreativeCoders.Core` for argument
  guards in libraries.
