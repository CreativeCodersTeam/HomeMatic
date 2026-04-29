using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Output;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;
using FakeItEasy;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests.Commands;

public class DataOutputCommandBaseAdditionalTests
{
    private sealed class TestOptions : IDataOutputOptions
    {
        public DataOutputFormat OutputFormat { get; init; } = DataOutputFormat.Auto;

        public string? OutputFile { get; init; }
    }

    private sealed class ConfigurableCommand : DataOutputCommandBase<string, TestOptions>
    {
        public ConfigurableCommand(
            IAnsiConsole console,
            IDataSerializerFactory factory,
            IDataOutputWriter writer)
            : base(console, factory, writer)
        {
        }

        public Func<TestOptions, Task<string>>? LoadFunc { get; init; }

        public Func<string, TestOptions, object?>? TransformFunc { get; init; }

        protected override Task<string> LoadDataAsync(TestOptions options)
            => LoadFunc is null ? Task.FromResult("data") : LoadFunc(options);

        protected override object TransformData(string data, TestOptions options)
            => TransformFunc is null ? data : TransformFunc(data, options)!;
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoadDataThrows_ExceptionPropagates()
    {
        // Arrange
        var sut = new ConfigurableCommand(
            A.Fake<IAnsiConsole>(),
            A.Fake<IDataSerializerFactory>(),
            A.Fake<IDataOutputWriter>())
        {
            LoadFunc = _ => throw new InvalidOperationException("boom")
        };

        // Act
        var act = async () => await sut.ExecuteAsync(new TestOptions());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransformReturnsNull_Throws()
    {
        // Arrange
        var sut = new ConfigurableCommand(
            A.Fake<IAnsiConsole>(),
            A.Fake<IDataSerializerFactory>(),
            A.Fake<IDataOutputWriter>())
        {
            TransformFunc = (_, _) => null
        };

        // Act
        var act = async () => await sut.ExecuteAsync(new TestOptions());

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenWriterThrows_ExceptionPropagates()
    {
        // Arrange
        var serializer = A.Fake<IDataSerializer>();
        A.CallTo(() => serializer.Serialize(A<object>._)).Returns("payload");

        var factory = A.Fake<IDataSerializerFactory>();
        A.CallTo(() => factory.Create(A<DataOutputFormat>._)).Returns(serializer);

        var writer = A.Fake<IDataOutputWriter>();
        A.CallTo(() => writer.ResolveFormat(A<DataOutputFormat>._, A<string?>._))
            .Returns(DataOutputFormat.Json);
        A.CallTo(() => writer.WriteAsync(A<string>._, A<string?>._))
            .ThrowsAsync(new IOException("write failed"));

        var sut = new ConfigurableCommand(A.Fake<IAnsiConsole>(), factory, writer);

        // Act
        var act = async () => await sut.ExecuteAsync(new TestOptions());

        // Assert
        await act.Should().ThrowAsync<IOException>().WithMessage("write failed");
    }
}
