using AwesomeAssertions;
using CreativeCoders.Cli.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Output;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Serialization;
using FakeItEasy;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests.Commands;

public class DataOutputCommandBaseTests
{
    private sealed class TestOptions : IDataOutputOptions
    {
        public DataOutputFormat OutputFormat { get; init; } = DataOutputFormat.Auto;

        public string? OutputFile { get; init; }
    }

    private sealed class TestCommand : DataOutputCommandBase<string, TestOptions>
    {
        private readonly string _data;

        public TestCommand(
            IAnsiConsole console,
            IDataSerializerFactory factory,
            IDataOutputWriter writer,
            string data)
            : base(console, factory, writer)
        {
            _data = data;
        }

        public int LoadCount { get; private set; }

        public int TransformCount { get; private set; }

        public int BeforeLoadCount { get; private set; }

        public int BeforeWriteCount { get; private set; }

        public int AfterWriteCount { get; private set; }

        public DataOutputFormat? LastResolvedFormat { get; private set; }

        protected override Task<string> LoadDataAsync(TestOptions options)
        {
            LoadCount++;

            return Task.FromResult(_data);
        }

        protected override object TransformData(string data, TestOptions options)
        {
            TransformCount++;

            return new { Wrapped = data };
        }

        protected override Task OnBeforeLoadAsync(TestOptions options)
        {
            BeforeLoadCount++;

            return Task.CompletedTask;
        }

        protected override Task OnBeforeWriteAsync(TestOptions options, DataOutputFormat resolvedFormat)
        {
            BeforeWriteCount++;
            LastResolvedFormat = resolvedFormat;

            return Task.CompletedTask;
        }

        protected override Task OnAfterWriteAsync(TestOptions options, DataOutputFormat resolvedFormat)
        {
            AfterWriteCount++;

            return Task.CompletedTask;
        }
    }

    private static (TestCommand Sut, IDataSerializer Serializer, IDataSerializerFactory Factory,
        IDataOutputWriter Writer) CreateSut(
            string serialized,
            DataOutputFormat resolvedFormat,
            string data = "payload")
    {
        var console = A.Fake<IAnsiConsole>();
        var serializer = A.Fake<IDataSerializer>();
        A.CallTo(() => serializer.Serialize(A<object>._)).Returns(serialized);

        var factory = A.Fake<IDataSerializerFactory>();
        A.CallTo(() => factory.Create(A<DataOutputFormat>._)).Returns(serializer);

        var writer = A.Fake<IDataOutputWriter>();
        A.CallTo(() => writer.ResolveFormat(A<DataOutputFormat>._, A<string?>._))
            .Returns(resolvedFormat);

        var sut = new TestCommand(console, factory, writer, data);

        return (sut, serializer, factory, writer);
    }

    [Fact]
    public async Task ExecuteAsync_OrchestratesLoadTransformSerializeAndWrite()
    {
        // Arrange
        var (sut, serializer, factory, writer) = CreateSut("serialized", DataOutputFormat.Yaml);
        var options = new TestOptions { OutputFormat = DataOutputFormat.Auto, OutputFile = "out.yaml" };

        // Act
        var result = await sut.ExecuteAsync(options);

        // Assert
        result.Should().Be(CommandResult.Success);
        sut.LoadCount.Should().Be(1);
        sut.TransformCount.Should().Be(1);
        A.CallTo(() => writer.ResolveFormat(DataOutputFormat.Auto, "out.yaml")).MustHaveHappenedOnceExactly();
        A.CallTo(() => factory.Create(DataOutputFormat.Yaml)).MustHaveHappenedOnceExactly();
        A.CallTo(() => serializer.Serialize(A<object>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => writer.WriteAsync("serialized", "out.yaml")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_InvokesAllHooks()
    {
        // Arrange
        var (sut, _, _, _) = CreateSut("serialized", DataOutputFormat.Json);
        var options = new TestOptions();

        // Act
        await sut.ExecuteAsync(options);

        // Assert
        sut.BeforeLoadCount.Should().Be(1);
        sut.BeforeWriteCount.Should().Be(1);
        sut.AfterWriteCount.Should().Be(1);
        sut.LastResolvedFormat.Should().Be(DataOutputFormat.Json);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullOptions_Throws()
    {
        // Arrange
        var (sut, _, _, _) = CreateSut("serialized", DataOutputFormat.Json);

        // Act
        var act = async () => await sut.ExecuteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullConsole_Throws()
    {
        // Arrange
        var factory = A.Fake<IDataSerializerFactory>();
        var writer = A.Fake<IDataOutputWriter>();

        // Act
        var act = () => new TestCommand(null!, factory, writer, "x");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullFactory_Throws()
    {
        // Arrange
        var console = A.Fake<IAnsiConsole>();
        var writer = A.Fake<IDataOutputWriter>();

        // Act
        var act = () => new TestCommand(console, null!, writer, "x");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullWriter_Throws()
    {
        // Arrange
        var console = A.Fake<IAnsiConsole>();
        var factory = A.Fake<IDataSerializerFactory>();

        // Act
        var act = () => new TestCommand(console, factory, null!, "x");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
