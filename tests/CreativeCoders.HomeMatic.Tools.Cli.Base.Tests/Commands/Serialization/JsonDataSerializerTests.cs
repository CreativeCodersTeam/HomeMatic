using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests.Commands.Serialization;

public class JsonDataSerializerTests
{
    private sealed class SampleData
    {
        public string FirstName { get; set; } = string.Empty;

        public int? Age { get; set; }

        public string? OptionalNote { get; set; }
    }

    [Fact]
    public void Format_Returns_Json()
    {
        // Arrange
        var sut = new JsonDataSerializer();

        // Act
        var format = sut.Format;

        // Assert
        format.Should().Be(DataOutputFormat.Json);
    }

    [Fact]
    public void Serialize_WithObject_ProducesIndentedCamelCaseJson()
    {
        // Arrange
        var sut = new JsonDataSerializer();
        var data = new SampleData { FirstName = "Alice", Age = 42 };

        // Act
        var result = sut.Serialize(data);

        // Assert
        result.Should().Contain("\"firstName\": \"Alice\"");
        result.Should().Contain("\"age\": 42");
        result.Should().Contain("\n");
    }

    [Fact]
    public void Serialize_WithNullProperty_OmitsTheProperty()
    {
        // Arrange
        var sut = new JsonDataSerializer();
        var data = new SampleData { FirstName = "Bob", Age = null, OptionalNote = null };

        // Act
        var result = sut.Serialize(data);

        // Assert
        result.Should().NotContain("optionalNote");
        result.Should().NotContain("age");
    }

    [Fact]
    public void Serialize_WithCollection_IncludesAllItems()
    {
        // Arrange
        var sut = new JsonDataSerializer();

        // Act
        var result = sut.Serialize(new[] { "a", "b", "c" });

        // Assert
        result.Should().Contain("\"a\"");
        result.Should().Contain("\"b\"");
        result.Should().Contain("\"c\"");
    }

    [Fact]
    public void Serialize_WithNestedObjectContainingNullProperty_OmitsNullProperty()
    {
        // Arrange
        var sut = new JsonDataSerializer();
        var data = new { Outer = new SampleData { FirstName = "X", Age = null, OptionalNote = null } };

        // Act
        var result = sut.Serialize(data);

        // Assert
        result.Should().Contain("\"firstName\": \"X\"");
        result.Should().NotContain("optionalNote");
        result.Should().NotContain("\"age\"");
    }

    [Fact]
    public void Serialize_WithNullData_Throws()
    {
        // Arrange
        var sut = new JsonDataSerializer();

        // Act
        var act = () => sut.Serialize(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
