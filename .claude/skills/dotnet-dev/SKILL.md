---
name: dotnet-dev
description: >
  Use when asked to implement, extend, or change a feature, user story,
  requirement, or bug fix in a .NET / C# project — any task that produces or
  modifies C# production code, tests, or documentation. Use when a .NET change
  request arrives, before writing any code.
---

# dotnet-dev — Comprehensive C# Requirement Implementation

A strict, gated workflow for implementing requirements end-to-end in .NET / C#.
Every phase ends in a confirmation gate; every applicable `dotnet-*` skill is a
mandatory binding. The workflow exists to survive exactly the conditions —
deadlines, "trivial" framing, "skip the ceremony" — under which it is most
tempting to abandon.

## Core Principle

**Violating the letter of this workflow is violating its spirit.** The phases,
gates, point-by-point clarification, and skill bindings are not ceremony to be
proportioned to task size. They are the work. A one-line endpoint runs the same
workflow as a subsystem — at speed, never collapsed.

## CRITICAL RULES (read before every phase)

1. **NO COMMITS.** Never run `git commit`, `git add -A`/`.`, branch, tag, or
   push. The user commits manually. If asked to commit, skip it and say so.

2. **URGENCY AND TRIVIALITY WAIVE NOTHING.** "It's trivial", "I'm in a hurry",
   "demo in 30 minutes", "skip the clarification dance", "no need for the full
   process", "just bang it out", "one pass is fine" are NOT permission to skip a
   phase, a gate, a clarification point, or a skill binding. Acknowledge the
   deadline, then run the workflow **at speed** — do not shrink it. The only
   lawful way to reduce a binding's work is an objective `n/a` (see below);
   user preference, pressure, and perceived size are never valid `n/a` reasons.
   An explicit, informed user waiver is a different thing from pressure — see
   *Waiver vs. `n/a` vs. silent skip*.

3. **EVERY PHASE ENDS IN A CONFIRMATION GATE.** At the end of each phase you
   MUST present a summary and STOP. Do not start the next phase until the user
   explicitly confirms. **Batching is a violation** — you may not present
   multiple phases at once, "pre-run" the next phase, or ask for one combined
   confirmation at the end. One phase → one summary → one wait.

4. **`dotnet-*` SKILLS ARE MANDATORY BINDINGS.** When a binding applies, you
   MUST invoke that skill via the `Skill` tool, in this conversation, for this
   task, **before** producing the corresponding artifact. Listing it,
   paraphrasing it, "knowing it already", or having invoked it for a previous
   task does NOT count. Invocation must shape the artifact, not certify it
   afterward.

## Phase Flow

```
Phase 1: Requirement Review              → GATE
Phase 2: Clarification (8 points)        → GATE
Phase 3: Task Breakdown                  → GATE
Phase 4: Implementation + App Smoke-Check ◄──┐  → GATE
Phase 5: Code Review (dotnet-reviewer)       │
   └─ rework needed? ───────────────────────►┘
   └─ all good → GATE
Phase 6: Final Summary
```

## Skill Map — Mandatory Bindings

| Binding | Skill | Applies in | When |
|---|---|---|---|
| Core C# / DI / Options / config / modern C# idioms | `dotnet-fundamentals` | Phase 4 | always, for production code |
| ASP.NET Core (controllers, minimal APIs, middleware, auth, ProblemDetails, OpenAPI) | `dotnet-aspnet` | Phase 4 | when ASP.NET Core is touched |
| EF Core (DbContext, entities, LINQ, migrations) | `dotnet-ef-core` | Phase 4 | when EF Core is touched |
| SDK / typed HTTP client generation | `dotnet-sdk-builder` | Phase 4 | when building a typed SDK / client |
| External / platform / NuGet API surface lookup | `dotnet-inspect` | Phase 1, 4 | when an external/platform API is involved |
| Tests | `dotnet-tester` | Phase 4 | always, when code is written or changed |
| XML documentation | `dotnet-xmldocs` | Phase 4 | for any public API addition/change |
| NuGet packages | `dotnet-nuget-manager` | Phase 4 | any package add/remove/version change |
| Code review | `dotnet-reviewer` | Phase 5 | always |
| Router (tie-breaker) | `dotnet` | any | when the right sub-skill is not obvious |

**Correct names matter:** `dotnet-ef-core` and `dotnet-nuget-manager` (not
`ef-core` / `nuget-manager`). A binding is fulfilled only when its skill has
fired via the `Skill` tool for this task. A binding applies whether you do the
work yourself or dispatch a sub-agent — self-execution never waives it, and a
sub-agent's invocation never waives the main agent's own follow-up edits.

**Tooling:** for code navigation/exploration follow the project + global rules
(Serena first, then tokensave; built-in/`Explore` agents only in the documented
carve-outs). Do NOT use `Explore` agents for code research when tokensave is
available.

---

## Phase 1 — Requirement Review

1. Read the requirement. Extract explicit AND implicit acceptance criteria.
2. **Find gaps, contradictions, and open questions** — list them explicitly. A
   review that surfaces no gaps on an ambiguous requirement is a failure; if the
   requirement truly is unambiguous, say so explicitly rather than skipping.
3. Analyze scope: affected `*.csproj`, components, files; existing tests/docs.
4. Read `CLAUDE.md`, `AGENTS.md`, `.editorconfig`, `Directory.Build.props`,
   `Directory.Packages.props` and follow the conventions found.
5. Invoke `dotnet-inspect` if any external/platform API surface must be verified.
6. **Determine the preliminary Skill Map** for this task and show it as a table.

**GATE 1 — STOP.** Present: requirement restated, acceptance criteria,
gaps/contradictions/open questions, affected scope, and the preliminary Skill
Map table. Wait for confirmation.

## Phase 2 — Clarification (8 points, one round-trip per point)

Clarify the following **one point at a time** — present point _N_, propose a
concrete default/assumption, ask the open question, and **wait for the user's
answer before moving to point _N+1_.** You may NOT batch points into one
message, NOR skip a point silently. A point that objectively does not apply is
presented with `n/a — <code-referenced reason>` and still acknowledged.

| # | Item | Must clarify |
|---|---|---|
| 1 | Project & folder structure | Target `*.csproj`, namespace, folder layout, new projects yes/no |
| 2 | Architecture & layering | Layers (controller/service/repo or vertical slice), DI lifetimes, public vs internal surface |
| 3 | Naming conventions | Type/method/file/namespace names, suffixes (`Service`, `Handler`, `Options`, `Async`), test naming |
| 4 | Public API / contracts | DTO shape, request/response, ProblemDetails, versioning, OpenAPI annotations |
| 5 | Errors & edge cases | Exception strategy, Result vs throws, validation style, logging granularity |
| 6 | Test strategy | Unit and/or integration, mock boundaries, naming, fakes vs real dependencies, coverage expectation |
| 7 | Persistence / EF Core | Entity shape, migrations yes/no, owned types, indexes, query style (LINQ vs spec) |
| 8 | Dependencies / NuGet | Allowed/forbidden packages, Central Package Management versions |

**GATE 2 — STOP.** Present: every clarified decision (all 8 points) and the
**finalized** Skill Map (updated from the answers — e.g. points 7/8 confirm
`dotnet-ef-core` / `dotnet-nuget-manager`). Wait for confirmation.

## Phase 3 — Task Breakdown

1. Break the requirement into discrete implementation tasks.
2. Each task covers **production code + tests + documentation**.
3. **Each task publishes a Skill-prerequisite checklist** (from the Skill Map):

   ```
   Task #N: <subject>
     Skill prerequisites:
       [ ] dotnet-fundamentals
       [ ] dotnet-aspnet
       [ ] dotnet-tester
       [ ] dotnet-xmldocs
   ```

4. Define dependencies and which tasks may run in parallel via sub-agents.

**GATE 3 — STOP.** Present the task list with per-task checklists, dependency
order, and parallelization. Wait for confirmation.

## Phase 4 — Implementation + App Smoke-Check

For each task (or parallel group):

1. **Step 0 — invoke every required `dotnet-*` skill for this task** before any
   `Write`/`Edit`/code-producing `Bash`. One `Skill(...)` call per binding, no
   collapsing. Re-invoke per task; previous-task invocations do not carry over.
   Then follow each skill's workflow when producing its artifact.
2. Implement production code (`dotnet-fundamentals` + stack skill), tests
   (`dotnet-tester`), XML docs (`dotnet-xmldocs`), package changes
   (`dotnet-nuget-manager`).
3. Run `dotnet build` and the test suite. Fix failures.
4. **App Smoke-Check** — if a runnable application exists, start it and verify
   it actually runs:
   - **Applies when** the solution has an executable project (`OutputType=Exe`,
     ASP.NET Core Web/API, Worker Service). **`n/a` only when** the solution is
     library-only (no executable project) — state that as the reason.
   - Start the app **in the background** (a server does not exit on its own),
     check startup logs / health endpoint / a representative request, then shut
     it down cleanly. Never block on a foreground long-running process.

**GATE 4 — STOP.** Present implemented tasks, build/test result, and the
App-Smoke-Check outcome (or its `n/a` reason). Wait for confirmation.

## Phase 5 — Code Review

1. Launch a code-review **sub-agent** that uses `dotnet-reviewer`. Invoke
   `dotnet-reviewer` via the `Skill` tool in the main conversation **and** pass
   it to the sub-agent prompt. Inline self-review does NOT satisfy this phase —
   `dotnet-reviewer` produces a severity-tagged Markdown report under
   `docs/reviews/`.
2. Evaluate findings:
   - **Rework needed** → create new tasks (each with its own Skill checklist)
     and return to **Phase 4**. After fixing, re-run Phase 5 (rework loop).
   - **All good** → proceed.

**GATE 5 — STOP.** Present the review findings, the report path, and your
rework/no-rework decision. Wait for confirmation.

## Phase 6 — Final Summary

1. Files created/modified; what was implemented and why.
2. Tests added/updated; documentation changes; package changes.
3. Decisions/trade-offs; anything the user should verify before committing.
4. **Skill-Invocation Log** (mandatory audit trail — see below).
5. Reminder: *the user commits; this workflow creates no commits.*

---

## `n/a` Criteria — Strict

A binding or clarification point may be `n/a` **only** when an objective
technical fact makes the work empty for this task. Valid reasons cite the
codebase, never the user:

- ✅ `n/a — task touches no public API members (internal sealed class)` — `dotnet-xmldocs`
- ✅ `n/a — no NuGet package added/removed/version-changed` — `dotnet-nuget-manager`
- ✅ `n/a — solution is library-only, no executable project` — App Smoke-Check
- ✅ `n/a — scanned changed files; no DbContext/IQueryable/migration` — `dotnet-ef-core`
- ❌ NOT valid: `n/a — user said no tests`
- ❌ NOT valid: `n/a — too small / trivial / one-liner / just a lambda`
- ❌ NOT valid: `n/a — single ToListAsync, doesn't need the skill`
- ❌ NOT valid: `n/a — no test project exists` (creating it is part of the task)
- ❌ NOT valid: `n/a — demo, no time`

If you cannot phrase the reason as "no `<artifact>` exists in this task because
`<code-referenced fact>`", it is NOT `n/a` — invoke the skill and do the work.

**No avoidance-driven `n/a`.** You may not reshape the implementation to escape
a binding — e.g. hand-rolling code to dodge `dotnet-nuget-manager`, inlining a
public member to dodge `dotnet-xmldocs`, or collapsing a service into private
code to dodge `dotnet-tester`. The binding follows the natural shape of the
work, not a shape chosen to minimise bindings.

## Waiver vs. `n/a` vs. silent skip

Three distinct cases — keep them apart. Conflating them is how the workflow
silently collapses.

- **Implicit pressure** — "it's trivial", "I'm in a hurry", "demo in 30
  minutes", "skip the ceremony", "this is how we work here", "nobody runs all
  this". **Waives NOTHING.** Acknowledge it and run the full workflow at speed.
  These are exactly the conditions the workflow exists to survive (CRITICAL
  RULE 2). Treat them as no-ops on scope.

- **Explicit, informed user waiver** — the user, *after being told what the step
  protects and what gap skipping it creates*, directly says "skip phase X / the
  gates / the clarification / the tests / the review." This **may be honored**
  (your global rule: explicit user instructions take precedence), but only on
  these terms:
  1. **State the cost first.** Name what the skipped step would have caught
     before you skip it. No silent compliance.
  2. **Record it as a waiver, never as `n/a`.** A waiver means "the user chose
     to skip this"; `n/a` means "the work is objectively empty." Stamping `n/a`
     on a waived step makes the audit trail lie about *why* it was skipped.
     Log it as: `waived — explicit user instruction (<who>, <when>); not a
     technical n/a; gap: <what is now uncovered>`.
  3. **Some steps still run regardless.** Invoking the relevant `dotnet-*`
     skills before writing code costs the user nothing and keeps the code
     matching repo conventions — keep them even under a waiver.

- **Objective `n/a`** — the work is empty for a code-referenced reason (see
  `n/a` Criteria). This is the only state that requires no user sign-off.

**The line:** vague urgency is not a waiver. A waiver is the user explicitly
electing to skip a *named* step after hearing its cost. When in doubt, ask the
user to confirm the waiver explicitly — do not infer one from pressure.

## Artifact-Substance Bar

Invoking a skill is necessary but not sufficient — the artifact must reflect it:

- A test that asserts nothing about the change (`Assert.True(true)`, a smoke
  test with no behavioral assertion) does not satisfy `dotnet-tester`.
- Empty `<summary />` or name-echoing docs do not satisfy `dotnet-xmldocs`.
- Hand-editing `<PackageReference>` after "invoking" `dotnet-nuget-manager` does
  not satisfy it — use the `dotnet` CLI as the skill prescribes.
- A "review" with no `docs/reviews/` report does not satisfy `dotnet-reviewer`.

If an artifact misses the bar, the binding is `[!]`, not `[x]`.

## Skill-Invocation Log (Phase 6)

Reproduce each task's checklist, every entry resolved with evidence:

- `[x] <skill> — invoked at <evidence>` — a verifiable ordered turn (e.g.
  "before Write of `UsersController.cs`"). "Considered"/"applied" is not
  evidence. Invocation AFTER the artifact is `[!]`, not `[x]`.
- `[n/a] <skill> — did not apply (<code-referenced reason>)`.
- `[!] <skill> — NOT invoked` — a violation. `[!]` is **not a shipping path**:
  name it, mark the task INCOMPLETE, STOP, and offer to re-enter Phase 4 to run
  the missing skill's workflow on the artifact. Phase 6 does not complete while
  any `[!]` is present.

## Red Flags — STOP and run the workflow

| Rationalization | Reality |
|---|---|
| "It's trivial / one endpoint — phases & gates are overkill" | Size does not scale the workflow. Run all phases at speed. Gate after each. |
| "Demo in 30 min / I'm in a hurry — one pass, no gates" | Urgency waives nothing (CRITICAL RULE 2). Acknowledge the deadline, keep the gates. |
| "Lead waived the clarification dance" | The 8 points are mandatory, one round-trip each. User waiver is not a valid skip. |
| "Ambiguities aren't blocking — I'll state assumptions instead of asking" | Stating assumptions ≠ clarifying. Present each point and wait for the answer. |
| "I'll batch all phases / ask one combined confirmation" | Batching is a gate violation. One phase → one summary → one wait. |
| "This is a single ToListAsync — `dotnet-ef-core` isn't needed" | Touching EF Core triggers the binding regardless of LINQ simplicity. Invoke it. |
| "Not a public library API, so skip `dotnet-xmldocs`" | `n/a` only for objectively internal members. Public additions/changes go through it. |
| "User said no tests" | User preference is not a valid `n/a`. Either an objective code-referenced `n/a` exists or `dotnet-tester` is required; if the user insists, name it as a violation and let them decide. |
| "I'll self-review the diff inline instead of `dotnet-reviewer`" | Phase 5 requires the `dotnet-reviewer` sub-agent + `docs/reviews/` report. Inline self-review does not satisfy it. |
| "I'll hand-roll it to avoid adding a package" | Avoidance-driven `n/a` is forbidden. The binding follows the natural shape of the work. |
| "The owner told me to skip it — I'll mark those steps `n/a`" | A waiver is not an `n/a`. Honor an explicit informed waiver if given, but record it as `waived — user instruction`, never `n/a`, never silently. See *Waiver vs. `n/a`*. |
| "They pushed back / 'nobody does this here' — the workflow is waived" | Pushback, urgency, and social proof are not waivers. Only an explicit, informed skip of a *named* step counts. Vague urgency waives nothing — run at speed. |
| "I already know C# / the skill — invoking is ceremony" | Skills encode project conventions and modern idioms. Knowing ≠ invoking. Invoke it. |
| "The skill was invoked last task" | Invocations do not carry over. Re-invoke per task. |
| "The sub-agent invoked it — covers my own edits" | It does not. Re-invoke before your own follow-up Write/Edit. |
| "I'll log the `Skill(...)` call even though I ran it after the Write" | Evidence must be a real ordered turn. After-the-fact is `[!]`, not `[x]`. |
| "No executable project, but I'll skip the App-Smoke-Check check anyway" | Decide it explicitly: run it, or mark `n/a — library-only`. Don't drop it silently. |

---

For detailed per-phase guidance, see
[references/REFERENCE.md](references/REFERENCE.md).
