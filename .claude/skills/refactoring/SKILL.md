---
name: refactoring
description: Refactor code to improve structure, readability, and maintainability while preserving functionality
---

# Refactoring Skill

When refactoring code, follow these principles:

## Refactoring Approach

### 1. Understand First
- Read and understand the existing code thoroughly
- Identify the current behavior and purpose
- Review existing tests to understand expected behavior

### 2. Identify Code Smells
- Long methods or classes
- Duplicated code
- Complex conditionals
- Poor naming
- Tight coupling
- Low cohesion

### 3. Apply Refactoring Patterns
- Extract Method: Break down long methods
- Extract Class: Separate concerns
- Rename: Improve clarity
- Introduce Parameter Object: Reduce parameter lists
- Replace Conditional with Polymorphism
- Simplify Complex Expressions

### 4. Ensure Safety
- Run existing tests before refactoring
- Make small, incremental changes
- Run tests after each change
- Verify functionality is preserved

### 5. Improve Design
- Apply SOLID principles
- Reduce dependencies
- Improve abstraction levels
- Enhance readability

## Refactoring Workflow

1. Ensure tests exist and pass
2. Identify refactoring opportunity
3. Plan the refactoring steps
4. Make one small change at a time
5. Run tests after each change
6. Commit when tests pass
7. Repeat until refactoring is complete
