using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests.Commands.Serialization;

public class DataSerializerFactoryTests
{
    private static IDataSerializer FakeSerializer(DataOutputFormat format)
    {
        var serializer = A.Fake<IDataSerializer>();
        A.CallTo(() => serializer.Format).Returns(format);

        return serializer;
    }

    [Fact]
    public void Create_WithJsonFormat_ReturnsRegisteredJsonSerializer()
    {
        // Arrange
        var jsonSerializer = FakeSerializer(DataOutputFormat.Json);
        var yamlSerializer = FakeSerializer(DataOutputFormat.Yaml);
        var sut = new DataSerializerFactory(new[] { jsonSerializer, yamlSerializer });

        // Act
        var result = sut.Create(DataOutputFormat.Json);

        // Assert
        result.Should().BeSameAs(jsonSerializer);
    }

    [Fact]
    public void Create_WithYamlFormat_ReturnsRegisteredYamlSerializer()
    {
        // Arrange
        var jsonSerializer = FakeSerializer(DataOutputFormat.Json);
        var yamlSerializer = FakeSerializer(DataOutputFormat.Yaml);
        var sut = new DataSerializerFactory(new[] { jsonSerializer, yamlSerializer });

        // Act
        var result = sut.Create(DataOutputFormat.Yaml);

        // Assert
        result.Should().BeSameAs(yamlSerializer);
    }

    [Fact]
    public void Create_WithAuto_Throws()
    {
        // Arrange
        var sut = new DataSerializerFactory(new[] { FakeSerializer(DataOutputFormat.Json) });

        // Act
        var act = () => sut.Create(DataOutputFormat.Auto);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WhenFormatNotRegistered_Throws()
    {
        // Arrange
        var sut = new DataSerializerFactory(new[] { FakeSerializer(DataOutputFormat.Json) });

        // Act
        var act = () => sut.Create(DataOutputFormat.Yaml);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithEmptySerializers_ThrowsForAnyFormat()
    {
        // Arrange
        var sut = new DataSerializerFactory(Array.Empty<IDataSerializer>());

        // Act
        var act = () => sut.Create(DataOutputFormat.Json);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_WithNullSerializers_Throws()
    {
        // Arrange & Act
        var act = () => new DataSerializerFactory(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
