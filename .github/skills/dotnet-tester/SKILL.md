---
name: dotnet-tester
description: Writes, executes, and completes unit tests for C#/.NET code using xUnit, FakeItEasy, and AwesomeAssertions. Uses a second agent to identify missing test cases. Use when asked to create .NET tests or improve test coverage.
---

Write comprehensive unit tests for the specified code. Follow a multi-step process with automatic identification of missing test cases.

## Conventions

- **Test Framework**: xUnit
- **Mocking**: FakeItEasy
- **Assertions**: AwesomeAssertions (a fork of FluentAssertions with identical API — use `Should()` as usual)
- **Structure**: Each test method has Arrange/Act/Assert blocks, marked with comments
- **Language**: English for code, comments, and test names
- **Style**: Adopt the existing test style from nearby test files in the project

## Phase 1: Write Tests

1. **Analyze code**: Read the code to be tested and understand:
  - Public API (methods, properties)
  - Dependencies (what needs to be mocked?)
  - Different code paths (if/else, switch, exceptions)
  - Edge Cases (null, empty collections, boundary values)

2. **Identify test project**: Find the appropriate test project by looking for `*.Tests.csproj` files or `*Tests/` directories in the solution. Fall back to scanning for any project whose name ends in `.Tests` or `.Test`. Orient yourself to the existing project structure before adding files.

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

Start a **separate agent** that reads the production code and written tests, then returns a prioritized list of missing cases:

- **Edge Cases**: Null, empty strings/collections, boundary/maximum values
- **Error Paths**: Exceptions, timeouts, dependency errors
- **Boundary Conditions**: Off-by-one, integer overflow
- **Interactions**: Call order, multiple/concurrent calls
- **State Transitions**: Different initial states

Format: `[HIGH/MEDIUM/LOW] MethodName - Scenario: Description`

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
