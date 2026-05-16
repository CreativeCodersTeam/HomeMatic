# Implementer Skill — Detailed Reference

This document provides in-depth guidance for each phase of the implementer workflow.

---

## Phase 1 — Requirement Review (Detail)

### Goal

Ensure a thorough understanding of the requirement before any implementation begins.
Prevent wasted effort from misunderstood requirements or unclear scope.

### Steps

#### 1.1 Read the Requirement

- Read the full requirement text (issue, user story, ticket, or user message)
- Identify the core objective: What should change? What is the expected outcome?

#### 1.2 Identify Acceptance Criteria

- Extract explicit acceptance criteria from the requirement
- Derive implicit criteria from context (e.g., existing tests must still pass, existing API contracts must be honored)
- List edge cases that should be covered

#### 1.3 Clarify Ambiguities

- If anything is unclear, use the `ask_user` tool to ask targeted questions
- Do NOT assume answers to ambiguous requirements — always ask
- Common ambiguities to watch for:
  - Scope boundaries (what is in/out)
  - Error handling behavior
  - Performance expectations
  - Backward compatibility requirements

#### 1.4 Analyze the Codebase

- Identify files, modules, and components affected by the requirement
- Understand the existing architecture and patterns in the affected area
- Look for existing tests that cover the affected area
- Check for documentation that needs updating
- Use explore sub-agents for large codebase investigations

#### 1.5 Check for Project-Specific Resources

- Look for project-specific skills (in `.claude/skills/`, `.github/skills/`, or `.agents/skills/`)
- Check for project-specific agents (in `.claude/agents/`, `.github/agents/`, or `.agents/`)
- Review instruction files (CLAUDE.md, AGENTS.md, .github/copilot-instructions.md)
- Follow any project conventions and coding standards found

### Output

Present the user with:
- A summary of the requirement in your own words
- The identified acceptance criteria
- Any clarifying questions (if applicable)
- The affected areas of the codebase
- Any project-specific resources that will be used

Wait for user confirmation before proceeding to Phase 2.

---

## Phase 2 — Implementation Plan (Detail)

### Goal

Create a clear, trackable plan that breaks the requirement into discrete tasks.
Each task should be self-contained and include code, tests, and documentation.

### Steps

#### 2.1 Define Tasks

- Break the requirement into the smallest reasonable tasks
- Each task should be completable independently (or with clear dependencies)
- Use descriptive kebab-case IDs for task tracking
- Every task description must include enough detail to execute without referring back to the plan

#### 2.2 Task Structure

Each task MUST address these three aspects:

1. **Production Code:** What code changes are needed?
2. **Tests:** What tests must be written or updated?
3. **Documentation:** What documentation needs updating? (can be "none" if truly not applicable)

#### 2.3 Dependencies

- Identify which tasks depend on others
- Tasks without dependencies can be parallelized
- Common dependency patterns:
  - Data model changes before API changes
  - Core logic before integration
  - Shared utilities before consumers

#### 2.4 Parallelization Strategy

- Group independent tasks for parallel execution via sub-agents
- Consider resource constraints (e.g., database schema changes should not be parallelized)
- Prefer smaller, focused sub-agent tasks over large monolithic ones

### Output

Present the user with:
- The complete task list with descriptions
- A dependency graph (which tasks block which)
- The planned execution order
- Which tasks will be parallelized

Wait for user confirmation before proceeding to Phase 3.

---

## Phase 3 — Implementation (Detail)

### Goal

Execute the implementation plan using sub-agents for efficient parallel work.

### Steps

#### 3.1 Task Execution

For each task (or parallel group of independent tasks):

1. **Update task status** to `in_progress`
2. **Choose the right sub-agent type:**
   - `explore` — For codebase research and analysis
   - `task` — For running builds, tests, linters (returns brief summary on success, full output on failure)
   - `general-purpose` — For complex multi-step code changes
3. **Provide complete context** to the sub-agent:
   - What to implement
   - Which files to modify
   - What tests to write
   - What conventions to follow
   - Reference to project-specific skills/agents if relevant
4. **Review sub-agent output** before moving to next task
5. **Update task status** to `done`

#### 3.2 Code Quality

- Follow existing code style and conventions
- Do not introduce new dependencies unless explicitly required
- Keep changes minimal and focused on the requirement
- Use existing patterns found in the codebase

#### 3.3 Testing

- Write tests that cover the new/changed behavior
- Ensure existing tests still pass
- Use the project's existing test framework and patterns
- Cover edge cases identified in Phase 1

#### 3.4 Documentation

- Update inline code documentation where needed
- Update README or other docs if the change affects usage
- Keep documentation changes in sync with code changes

#### 3.5 Verification

After all tasks are complete:
- Run the full test suite using a `task` sub-agent
- Run any existing linters or type checkers
- Fix any failures before proceeding

### Important Constraints

- **NEVER commit code.** The user will commit when ready.
- **NEVER skip tests.** Every code change must have corresponding tests.
- **Respect existing patterns.** Do not refactor unrelated code.

---

## Phase 4 — Review (Detail)

### Goal

Ensure implementation quality through an automated code review before the user commits.

### Steps

#### 4.1 Launch Code Review

Launch a `code-review` sub-agent with these instructions:
- Review ALL changes made during this implementation session
- Compare changes against the original requirement and acceptance criteria
- Focus on substantive issues only (not style or formatting)

#### 4.2 Review Criteria

The review sub-agent checks for:

1. **Correctness:** Does the implementation satisfy the requirement and all acceptance criteria?
2. **Completeness:** Are there missing edge cases, error handling, or untested paths?
3. **Test Coverage:** Do tests adequately cover the new/changed code?
4. **Documentation:** Is documentation accurate and up to date?
5. **Code Quality:** Are there bugs, security issues, or logic errors?
6. **Consistency:** Does the code follow project conventions and patterns?

#### 4.3 Evaluate Findings

After the review:

- **No issues found:** Proceed to Phase 5
- **Minor issues found:** Fix them directly, then proceed to Phase 5
- **Significant issues found:**
  1. Create new tasks for each finding
  2. Return to Phase 3 to address them
  3. After fixing, run Phase 4 again (rework loop)

#### 4.4 Rework Loop

The rework loop (Phase 3 → Phase 4) continues until:
- The review finds no significant issues
- OR the user explicitly approves the current state

There is no hard limit on rework iterations, but if the same issues recur after 2 rework cycles, consult the user for guidance.

---

## Phase 5 — Summary (Detail)

### Goal

Provide a clear, comprehensive summary so the user knows exactly what was done and can review before committing.

### Steps

#### 5.1 Change Summary

Create a structured summary containing:

1. **Requirement:** Brief restatement of the original requirement
2. **Files Modified:** List all files that were created or changed
3. **Implementation Details:** What was implemented and key design decisions
4. **Tests Added/Updated:** List all test files and what they cover
5. **Documentation Changes:** List all documentation updates
6. **Decisions Made:** Any design decisions or trade-offs during implementation
7. **Review Notes:** Key points from the review phase
8. **Things to Check:** Anything the user should manually verify before committing

#### 5.2 Final Reminder

End with a clear reminder:

> All changes are ready for your review. When you are satisfied, please commit
> the changes yourself. The AI will not create any commits.

---

## General Guidelines

### Sub-Agent Best Practices

- Always provide **complete context** to sub-agents (they are stateless)
- Use `explore` agents for research, `task` agents for builds/tests, `general-purpose` for complex changes
- Launch independent tasks in **parallel** for efficiency
- Review sub-agent output before acting on it

### Git and Commit Rules

- **NEVER** run `git commit`, `git add`, or any commit-related commands
- **NEVER** create branches, tags, or push to remotes
- If the user or a tool requests a commit, **skip it** and inform the user:
  *"Committing is your responsibility. I've skipped this commit request."*
- You MAY use `git diff`, `git status`, `git log`, and other read-only git commands for analysis

### Handling Project-Specific Conventions

- Always check for instruction files at the start (CLAUDE.md, AGENTS.md, etc.)
- Look for project-specific skills that might provide specialized guidance
- Follow the project's existing patterns for code style, testing, and documentation
- When in doubt, ask the user about project conventions
