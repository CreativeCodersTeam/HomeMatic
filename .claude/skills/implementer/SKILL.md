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

**Usage rule:** If a slot is filled, you MUST use that skill for the matching work and pass
it as context to any sub-agent doing that work (sub-agents are stateless). If no skill
matches a slot, fall back to generic conventions and note the empty slot in the plan.

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
3. Each task MUST record which **capability slots** it uses (from Phase 1 discovery), so
   skill usage is part of the plan and verifiable
4. Define task dependencies (what must be done first)
5. Identify tasks that can be parallelized via sub-agents

**Output:** Task list with dependencies and per-task capability-slot assignments, ready for
implementation.

## Phase 3 — Implementation

Execute tasks using sub-agents for parallel work where possible:

1. For each task (or group of independent tasks):
   - Consult the task's assigned capability-slot skills BEFORE writing anything
   - Delegate to sub-agents (explore for research, task for builds/tests, general-purpose for
     complex changes), passing the relevant slot skill as context
   - Implement production code changes (use the `language-implementation` slot)
   - Write or update tests to cover the changes (use the `language-testing` slot)
   - Update relevant documentation (use the `language-docs` slot)
2. Run existing tests and linters to verify changes don't break anything (use the
   `build-deps` slot if filled)
3. Track task completion status

**Important:** Respect the project's existing conventions, patterns, and tooling. If a
capability slot is filled, you MUST use it; only fall back to generic work when the slot is
empty.

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

---

For detailed guidance on each phase, see [references/REFERENCE.md](references/REFERENCE.md).
