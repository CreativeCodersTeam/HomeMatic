using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Output;
using FakeItEasy;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Tests.Commands.Output;

public class DataOutputWriterTests
{
    [Theory]
    [InlineData("output.json", DataOutputFormat.Json)]
    [InlineData("Output.JSON", DataOutputFormat.Json)]
    [InlineData("/tmp/data.yaml", DataOutputFormat.Yaml)]
    [InlineData("data.YML", DataOutputFormat.Yaml)]
    public void ResolveFormat_WhenFileExtensionKnown_ReturnsFormatFromExtension(
        string outputFile, DataOutputFormat expected)
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());

        // Act
        var result = sut.ResolveFormat(DataOutputFormat.Json, outputFile);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ResolveFormat_WhenFileExtensionKnown_OverridesRequestedFormat()
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());

        // Act
        var result = sut.ResolveFormat(DataOutputFormat.Json, "result.yaml");

        // Assert
        result.Should().Be(DataOutputFormat.Yaml);
    }

    [Theory]
    [InlineData(DataOutputFormat.Json, DataOutputFormat.Json)]
    [InlineData(DataOutputFormat.Yaml, DataOutputFormat.Yaml)]
    [InlineData(DataOutputFormat.Auto, DataOutputFormat.Json)]
    public void ResolveFormat_WhenNoOutputFile_UsesRequestedOrDefault(
        DataOutputFormat requested, DataOutputFormat expected)
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());

        // Act
        var result = sut.ResolveFormat(requested, null);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ResolveFormat_WhenExtensionUnknown_FallsBackToRequestedFormat()
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());

        // Act
        var result = sut.ResolveFormat(DataOutputFormat.Yaml, "data.txt");

        // Assert
        result.Should().Be(DataOutputFormat.Yaml);
    }

    [Fact]
    public void ResolveFormat_WhenExtensionUnknownAndAuto_DefaultsToJson()
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());

        // Act
        var result = sut.ResolveFormat(DataOutputFormat.Auto, "data.txt");

        // Assert
        result.Should().Be(DataOutputFormat.Json);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task WriteAsync_WithoutOutputFile_WritesToConsole(string? outputFile)
    {
        // Arrange
        var console = A.Fake<IAnsiConsole>();
        var sut = new DataOutputWriter(console);

        // Act
        await sut.WriteAsync("payload", outputFile);

        // Assert
        A.CallTo(console)
            .Where(c => c.Method.Name == nameof(IAnsiConsole.Write))
            .MustHaveHappened();
    }

    [Fact]
    public async Task WriteAsync_WithOutputFile_WritesContentToFile()
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());
        var path = Path.Combine(Path.GetTempPath(), $"data-output-writer-{Guid.NewGuid():N}.json");

        try
        {
            // Act
            await sut.WriteAsync("hello", path);

            // Assert
            File.Exists(path).Should().BeTrue();
            (await File.ReadAllTextAsync(path)).Should().Be("hello");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task WriteAsync_WithNullContent_Throws()
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());

        // Act
        var act = async () => await sut.WriteAsync(null!, null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void ResolveFormat_WithEmptyOutputFile_BehavesLikeNull()
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());

        // Act
        var result = sut.ResolveFormat(DataOutputFormat.Yaml, string.Empty);

        // Assert
        result.Should().Be(DataOutputFormat.Yaml);
    }

    [Fact]
    public void ResolveFormat_WithMultipleDotsInFileName_UsesLastExtension()
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());

        // Act
        var result = sut.ResolveFormat(DataOutputFormat.Yaml, "data.backup.json");

        // Assert
        result.Should().Be(DataOutputFormat.Json);
    }

    [Fact]
    public async Task WriteAsync_ToNonExistentDirectory_Throws()
    {
        // Arrange
        var sut = new DataOutputWriter(A.Fake<IAnsiConsole>());
        var path = Path.Combine(
            Path.GetTempPath(),
            $"missing-dir-{Guid.NewGuid():N}",
            "out.json");

        // Act
        var act = async () => await sut.WriteAsync("data", path);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public void Constructor_WithNullConsole_Throws()
    {
        // Arrange & Act
        var act = () => new DataOutputWriter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
