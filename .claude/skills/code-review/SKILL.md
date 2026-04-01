---
name: code-review
description: Performs thorough code reviews covering quality, security, performance, and best practices. Use when asked to review a pull request, audit code changes, check for security vulnerabilities, assess code quality, or provide feedback on a diff. Outputs structured feedback with severity levels and actionable suggestions.
---

# Code Review Skill

## Review Checklist

### 1. Code Quality
- Check for code readability and maintainability
- Verify proper naming conventions
- Look for code duplication
- Assess function and class sizes
- Check for proper separation of concerns

### 2. Security Review
- Check for security vulnerabilities
- Verify input validation
- Look for potential injection attacks
- Check for exposed secrets or credentials
- Verify proper authentication and authorization

### 3. Performance
- Identify potential performance bottlenecks
- Check for inefficient algorithms or data structures
- Look for unnecessary database queries or API calls
- Verify proper resource management

### 4. Testing
- Verify test coverage
- Check test quality and relevance
- Ensure edge cases are tested
- Verify integration tests exist where needed

### 5. Documentation
- Check if public APIs are documented
- Verify complex logic has explanatory comments
- Ensure README and other docs are updated

## Output Format

Provide feedback in the following structure:
1. Summary of changes reviewed
2. Positive aspects of the code
3. Issues found (categorized by severity: Critical, Major, Minor)
4. Suggestions for improvement
5. Overall recommendation (Approve / Request Changes / Comment)
