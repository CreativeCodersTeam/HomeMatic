---
name: implementer
description: >
  Iterative implementation workflow for requirements. Use this skill when asked to
  implement a feature, user story, requirement, or change request. Guides through
  5 phases: requirement review, implementation planning, sub-agent-driven implementation
  (code, tests, documentation), code review with rework loop, and final summary.
  Never commits code — the user always commits manually.
allowed-tools: Read Grep Glob Edit Create Task
---

# Implementer — Iterative Requirement Implementation Flow

An iterative, structured workflow for implementing requirements end-to-end.
Covers production code, tests, and documentation updates in every cycle.

> **CRITICAL RULE — NO COMMITS:** You must NEVER commit code or create git commits.
> The user always commits manually. If asked to commit, skip that request and inform
> the user that committing is their responsibility.

> **CRITICAL RULE — USE SPECIALIZED SKILLS:** Never implement, test, document, or review
> with only your built-in knowledge when a specialized skill for the detected technology
> exists. Discover available skills at runtime and use them. This skill names NO concrete
> skills on purpose — it works with whatever skills exist now or are added later.

## Flow Overview

```
Phase 1: Requirement Review
    ↓
Phase 2: Implementation Plan
    ↓
Phase 3: Implementation (Sub-Agents) ◄──┐
    ↓                                    │
Phase 4: Review (Sub-Agent)              │
    ↓ (rework needed?)──────────────────►┘
    ↓ (all good)
Phase 5: Summary
```

## Skill Discovery & Capability Slots

This workflow relies on **discovering specialized skills at runtime** rather than naming
them. Concrete skill names change over time — names are never hardcoded here.

**How discovery works:**

1. **Detect the tech stack** by *signals* (manifest/build files, source extensions, configs),
   per affected file/module — repos may mix stacks. Examples like `*.csproj` → .NET or
   `package.json` → TypeScript are illustrative only; infer any stack from its signals
   (see [Phase 1.5](references/REFERENCE.md#phase-15)).
2. **List the available skills** using your runtime's skill-listing mechanism (do not assume
   a fixed directory).
3. **Classify each skill by its `description`** — match purpose and technology from the text,
   never from the name — and fill these **capability slots**:

| Slot | Filled by a skill whose description covers… | Used in |
|------|---------------------------------------------|---------|
| `language-implementation` | writing code for the detected stack | Phase 3 |
| `language-testing` | the stack's test framework / test conventions | Phase 3 |
| `language-docs` | the stack's documentation conventions | Phase 3 |
| `language-review` | reviewing code for the detected stack | Phase 4 |
| `build-deps` | builds / package & dependency management | Phase 3 |

Always show the capability slots to the user.
**Usage rule:** If a slot is filled, you MUST use that skill for the matching work and pass
it as context to any sub-agent doing that work (sub-agents are stateless). If no skill
matches a slot, fall back to generic conventions and note the empty slot in the plan. if you delegate work to a sub-agent,
you MUST pass the slot skill explicitly.

**Slot fulfillment is by invocation, not by listing.** A capability slot counts as fulfilled
only when its skill has been invoked via the `Skill` tool **in this conversation, for this
task**. Listing a skill in the slot table, paraphrasing its description, or relying on prior
knowledge of the technology does NOT count as fulfillment. The rule is identical whether you
do the work yourself or dispatch a sub-agent — self-execution does not waive the
invocation, and a sub-agent invocation does not waive it for the main agent's own
follow-up edits.

**A single task may have multiple slots filled at once** (e.g. implementation + testing +
documentation in one task, or two language-implementation slots when a stack mixes
sub-technologies). In that case every assigned slot must be invoked independently — one
slot's invocation never substitutes for another's, and you must not collapse the
invocations even when the same task touches all of them. Invocations from a previous task
do NOT carry over either; re-invoke at the start of every task where the slot applies.

## Phase 1 — Requirement Review

Analyze the requirement before any code is written:

1. Read and understand the requirement thoroughly
2. Identify acceptance criteria (explicit and implicit)
3. Clarify ambiguities — ask the user targeted questions using the ask_user tool
4. Identify affected components, files, and modules in the current codebase
5. Check for existing tests, documentation, and related code
6. **Detect the tech stack and run skill discovery** (see *Skill Discovery & Capability
   Slots*): build the capability-slot map for the affected areas

**Output:** Confirmed understanding of the requirement, resolved ambiguities, identified
scope, and the filled capability-slot map (with any empty slots noted).

## Phase 2 — Implementation Plan

Create a structured plan with trackable tasks:

1. Break the requirement into discrete implementation tasks
2. Each task MUST include all three aspects:
  - **Production code** changes
  - **Test** additions or updates
  - **Documentation** updates (if applicable)
3. **Each task MUST publish a Skill-prerequisite checklist** listing every capability slot
   it depends on (from the Phase 1 discovery). The checklist is part of the plan text the
   user sees; it is not optional, and it is not collapsed even when a task touches every
   slot. The format is:

   ```
   Task #N: <subject>
     Skill prerequisites (Step 0 of Phase 3):
       [ ] <slot-id>            — filled by "<skill name from discovery>"
       [ ] <other-slot-id>      — filled by "<other skill name>"
       [ ] <slot-id-with-gap>   — EMPTY (no matching skill; generic fallback)
   ```

   When Step 0 fires in Phase 3, each `[ ]` is replaced with `[x]` (or annotated with
   "n/a — slot empty" for empty slots). Multiple skills filling the same slot get their own
   line each. The same skill appearing in two different slots is two separate
   prerequisites — never deduplicated.
4. Define task dependencies (what must be done first)
5. Identify tasks that can be parallelized via sub-agents

**Output:** Task list with dependencies and a per-task Skill-prerequisite checklist, ready
for implementation. Any task missing its checklist is not a valid plan entry.

## Phase 3 — Implementation

Execute tasks using sub-agents for parallel work where possible:

1. For each task (or group of independent tasks):
  - **Step 0 — invoke every assigned slot skill.** Before any `Write`, `Edit`, or
    code-producing `Bash` call for this task, call the `Skill` tool **once per capability
    slot** the task is assigned to. If the task has multiple slots (e.g.
    `language-implementation` + `language-testing` + `language-docs` at the same time, or
    two slots of the same kind), invoke them all — one invocation never substitutes for
    another. Wait for each skill's content to load and follow its workflow when producing
    the corresponding artifact. A skill invocation made for a previous task does NOT carry
    over; re-invoke per task.
  - Delegate to sub-agents (explore for research, task for builds/tests, general-purpose
    for complex changes), passing the relevant slot skill(s) as context. Sub-agent
    dispatch is in addition to — not instead of — Step 0: the main agent still needs the
    slot's workflow loaded for any follow-up edits it makes itself.
  - Implement production code changes using the skill(s) filling the
    `language-implementation` slot.
  - Write or update tests using the skill(s) filling the `language-testing` slot.
  - Update relevant documentation using the skill(s) filling the `language-docs` slot.
2. Run existing tests and linters to verify changes don't break anything (use the
   `build-deps` slot if filled — invoke it the same way as any other slot).
3. Track task completion status. Only mark a task `completed` once every slot assigned to
   it has been invoked AND the corresponding artifact (code / tests / docs) has been
   produced or explicitly recorded as not-applicable for this task.

**Important:** Respect the project's existing conventions, patterns, and tooling. If a
capability slot is filled, you MUST use it; only fall back to generic work when the slot is
empty.

### Worked example — beginning a task with multiple slots

Suppose Task #N has three slots assigned: `language-implementation`, `language-testing`,
and `language-docs`, each filled by some discovered skill. The correct opening of the task
looks like this (skill names are placeholders — substitute whatever Phase 1 discovery
produced for each slot):

```
TaskUpdate(taskId=N, status=in_progress)
Skill(skill="<skill filling language-implementation>")   ← MUST fire before production-code Write/Edit
Skill(skill="<skill filling language-testing>")          ← MUST fire before test-code Write/Edit
Skill(skill="<skill filling language-docs>")             ← MUST fire before doc Write/Edit
# ...follow each invoked skill's workflow...
Write(file_path=".../<production file>", content=...)
Write(file_path=".../<test file>",       content=...)
Edit(file_path=".../<doc file>",         ...)
```

Notes:

- Three slots → three `Skill(...)` calls. Two slots → two calls. Five → five. No collapsing.
- If you dispatch a sub-agent for one of the slots, the `Skill(...)` call still happens
  here in the main conversation first, and the slot skill name is also passed to the
  sub-agent prompt.
- Omitting any `Skill(...)` line above is exactly the failure mode the rules in this
  section exist to prevent.

## Phase 4 — Review

Run a thorough code review using a sub-agent:

1. Launch a code-review sub-agent to analyze all changes made. If the `language-review` slot
   is filled, use that skill (it knows stack-specific pitfalls); otherwise use a generic
   review. When both a stack-specific and a generic review skill exist, combine them.
2. The review checks for:
  - Correctness and completeness against the requirement
  - Test coverage for new/changed code
  - Documentation accuracy
  - Code quality, potential bugs, and security issues
3. Evaluate review findings:
  - **Rework needed:** Create new tasks for findings and return to **Phase 3**
  - **All good:** Proceed to **Phase 5**

## Phase 5 — Summary

Provide a comprehensive summary of all work done:

1. List all files created or modified
2. Describe what was implemented and why
3. List all tests added or updated
4. List all documentation changes
5. Note any decisions made during implementation
6. Highlight anything the user should review before committing
7. **Publish the Skill-invocation log.** For every task in the plan, reproduce the
   Skill-prerequisite checklist from Phase 2 with each entry resolved. Each line MUST
   carry one of three states, with evidence:

  - `[x] <slot-id> — invoked at <evidence>` — `<evidence>` is a verifiable pointer such
    as "turn N", "before Write of `<file>`", or the exact `Skill(skill="…")` call. Generic
    phrases like "considered" or "applied" are NOT acceptable evidence.
  - `[n/a] <slot-id> — empty in Phase 1 discovery; generic fallback used` — only valid
    when Phase 1 left the slot empty.
  - `[!] <slot-id> — NOT invoked` — a workflow violation. Whenever this state appears you
    MUST (a) name it explicitly here as a violation, (b) NOT mark the affected task as
    truly complete in your prose summary, and (c) recommend a concrete follow-up pass
    that re-runs the missing slot's workflow on the produced artifact.

   The log is mandatory even when every slot was invoked correctly — it serves as the
   audit trail for the user. If the plan contained no tasks (e.g. requirement was a
   no-op), state that explicitly instead of omitting the log.

> **Reminder:** The user will commit the changes themselves. Do NOT create any commits.

## Red Flags — Do Not Skip Specialized Skills

If you catch yourself thinking any of these, STOP and use the discovered slot skill:

| Rationalization | Reality |
|-----------------|---------|
| "I already know this language well" | The skill may encode project-specific conventions you don't know. Consult it. |
| "It's a tiny change, no skill needed" | Small changes still follow the stack's patterns. Use the slot. |
| "I'll just use the generic approach" | Generic is the fallback ONLY when no slot skill exists. |
| "Discovery takes too long" | Discovery is one listing + classification. Skipping it produces off-convention code. |
| "The sub-agent will figure it out" | Sub-agents are stateless — pass them the slot skill explicitly. |
| "I already listed the slot in the plan" | A listed slot is not an invoked skill. The slot is only fulfilled when the `Skill` tool has fired in this conversation for this task. |
| "I'll just do this small bit myself instead of dispatching" | Self-execution does NOT waive the slot requirement. Invoke the slot skill first, then write. |
| "One slot covers everything in this task" | Each filled slot is its own workflow. Implementation, testing, documentation, build, and review slots must each be invoked when assigned — never collapse them. |
| "The slot was invoked in the previous task" | Invocations do NOT carry over between tasks. Re-invoke at the start of every task the slot applies to. |

---

For detailed guidance on each phase, see [references/REFERENCE.md](references/REFERENCE.md).
