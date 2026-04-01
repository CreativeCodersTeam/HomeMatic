---
name: refactoring
description: Refactors existing code to improve structure, readability, and maintainability without changing behavior. Use when asked to clean up code, reduce duplication, apply SOLID principles, or address code smells (long methods, tight coupling, poor naming). Always verifies existing tests pass before and after changes.
---

# Refactoring Skill

Unless explicitly asked to refactor code directly, generate a refactoring plan first and only apply it if the user approves.

## Workflow

1. **Understand**: Read existing code, identify purpose and behavior, review existing tests
2. **Identify code smells**: Long methods/classes, duplicated code, complex conditionals, poor naming, tight coupling, low cohesion
3. **Plan**: Select appropriate refactoring patterns:
   - Extract Method / Extract Class
   - Rename for clarity
   - Introduce Parameter Object
   - Replace Conditional with Polymorphism
   - Simplify Complex Expressions
   - Apply SOLID principles, reduce dependencies, improve abstraction levels
4. **Execute safely**: Make small, incremental changes. Run tests after each change.
5. **Verify**: Ensure all tests pass and functionality is preserved
