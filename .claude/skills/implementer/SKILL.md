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

## Phase 1 — Requirement Review

Analyze the requirement before any code is written:

1. Read and understand the requirement thoroughly
2. Identify acceptance criteria (explicit and implicit)
3. Clarify ambiguities — ask the user targeted questions using the ask_user tool
4. Identify affected components, files, and modules in the current codebase
5. Check for existing tests, documentation, and related code
6. Note any project-specific skills or agents that should be consulted

**Output:** Confirmed understanding of the requirement, resolved ambiguities, identified scope.

## Phase 2 — Implementation Plan

Create a structured plan with trackable tasks:

1. Break the requirement into discrete implementation tasks
2. Each task MUST include all three aspects:
   - **Production code** changes
   - **Test** additions or updates
   - **Documentation** updates (if applicable)
3. Define task dependencies (what must be done first)
4. Identify tasks that can be parallelized via sub-agents
5. Check for project-specific skills, agents, or conventions that apply

**Output:** Task list with dependencies, ready for implementation.

## Phase 3 — Implementation

Execute tasks using sub-agents for parallel work where possible:

1. For each task (or group of independent tasks):
   - Delegate to sub-agents (explore for research, task for builds/tests, general-purpose for complex changes)
   - Implement production code changes
   - Write or update tests to cover the changes
   - Update relevant documentation
2. Run existing tests and linters to verify changes don't break anything
3. Track task completion status
4. If project-specific skills or agents are available, use them for specialized work

**Important:** Respect the project's existing conventions, patterns, and tooling.

## Phase 4 — Review

Run a thorough code review using a sub-agent:

1. Launch a code-review sub-agent to analyze all changes made
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

---

For detailed guidance on each phase, see [references/REFERENCE.md](references/REFERENCE.md).
