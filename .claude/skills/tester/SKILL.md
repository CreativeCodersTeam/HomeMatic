---
name: tester
description: Writes, executes, and completes unit tests for C#/.NET code. Uses a second agent to identify missing test cases. Use this skill when you are asked to create tests or improve test coverage.
---

Write comprehensive unit tests for the specified code. Follow a multi-step process with automatic identification of missing test cases.

## Conventions

- **Test Framework**: xUnit
- **Mocking**: FakeItEasy
- **Assertions**: AwesomeAssertions
- **Structure**: Each test method has Arrange/Act/Assert blocks, marked with comments
- **Language**: English for code, comments, and test names
- **Style**: Adopt the existing test style from nearby test files in the project

## Phase 1: Write Tests

1. **Analyze code**: Read the code to be tested and understand:
  - Public API (methods, properties)
  - Dependencies (what needs to be mocked?)
  - Different code paths (if/else, switch, exceptions)
  - Edge Cases (null, empty collections, boundary values)

2. **Identify test project**: Find the appropriate test project under `source/Test/`. Orient yourself to the existing project structure.

3. **Create tests**: Write tests following this pattern:

```csharp
public class MyClassTests
{
    [Fact]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var dependency = A.Fake<IDependency>();
        A.CallTo(() => dependency.DoSomething()).Returns(expectedValue);
        var sut = new MyClass(dependency);

        // Act
        var result = sut.MethodUnderTest(input);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("input1", "expected1")]
    [InlineData("input2", "expected2")]
    public void MethodName_WithVariousInputs_ReturnsExpected(string input, string expected)
    {
        // Arrange
        var sut = new MyClass();

        // Act
        var result = sut.MethodUnderTest(input);

        // Assert
        result.Should().Be(expected);
    }
}
```

4. **Cover test categories**:
  - Happy Path (normal success case)
  - Error handling (exceptions, invalid inputs)
  - Null/empty inputs
  - Boundary values (boundary conditions)
  - Dependency behavior (mocks, different return values)

## Phase 2: Execute Tests

1. Run `dotnet test` in the relevant test project
2. Analyze the results:
  - On **failures**: Identify the cause and fix the test or test setup
  - On **success**: Continue to Phase 3
3. Repeat until all tests are green. Don't fakely pass tests. If a test is too complex to set up, consider if it should be refactored or if the code under test should be made more testable.

## Phase 3: Identify Missing Test Cases

Start a **separate agent** that analyzes the written tests and identifies missing cases.

The agent should:

1. Read the production code to be tested
2. Read the written tests
3. Return a prioritized list of missing test cases, categorized by:

  - **Missing Edge Cases**: Null values, empty strings, empty collections, maximum values
  - **Missing Error Paths**: Exception scenarios, timeout behavior, errors in dependencies
  - **Missing Boundary Conditions**: Boundary values, off-by-one, integer overflow
  - **Missing Interactions**: Order of calls, multiple calls, concurrent access
  - **Missing State Transitions**: Different initial states, state machines

Return format:
```
1. [HIGH] MethodName - Scenario: Description of missing test
2. [MEDIUM] MethodName - Scenario: Description of missing test
3. [LOW] MethodName - Scenario: Description of missing test
```

## Phase 4: Add Missing Tests

1. Implement the identified missing tests (prioritized: HIGH → MEDIUM → LOW)
2. Run `dotnet test` again
3. Fix any errors
4. Ensure all tests are green

## Output Format

At the end, provide a summary:

```
### Test Result

**Phase 1**: X tests written
**Phase 2**: All tests green ✅
**Phase 3**: Y missing cases identified (Z High, W Medium, V Low)
**Phase 4**: Y additional tests implemented, all green ✅

**Total**: X + Y tests, all passed
```

## Important Notes

- Do **not write tests for trivial getters/setters** without logic
- Do **not mock value types** or simple DTOs – create real instances
- Test **behavior**, not implementation details
- Use **descriptive test names** in the format `MethodName_Scenario_ExpectedBehavior`
- For `[Theory]` tests: Use `[InlineData]` for simple types, `[MemberData]` for complex objects
- Use `A.CallTo(...).MustHaveHappened()` sparingly – only when the call is the expected behavior
