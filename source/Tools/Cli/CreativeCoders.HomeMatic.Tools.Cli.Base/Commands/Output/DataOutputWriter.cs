using CreativeCoders.Core;
using CreativeCoders.Core.IO;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Output;

/// <summary>
/// Default <see cref="IDataOutputWriter"/> implementation. Writes content either to a file via
/// <see cref="FileSys"/> or to the provided <see cref="IAnsiConsole"/>.
/// </summary>
public class DataOutputWriter : IDataOutputWriter
{
    private readonly IAnsiConsole _console;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataOutputWriter"/> class.
    /// </summary>
    /// <param name="console">The console used for stdout output.</param>
    public DataOutputWriter(IAnsiConsole console)
    {
        _console = Ensure.NotNull(console);
    }

    /// <inheritdoc />
    public DataOutputFormat ResolveFormat(DataOutputFormat requestedFormat, string? outputFile)
    {
        var fromExtension = TryGetFormatFromExtension(outputFile);

        if (fromExtension.HasValue)
        {
            return fromExtension.Value;
        }

        return requestedFormat == DataOutputFormat.Auto
            ? DataOutputFormat.Json
            : requestedFormat;
    }

    /// <inheritdoc />
    public async Task WriteAsync(string content, string? outputFile)
    {
        Ensure.NotNull(content);

        if (string.IsNullOrWhiteSpace(outputFile))
        {
            _console.WriteLine(content);
            return;
        }

        await FileSys.File.WriteAllTextAsync(outputFile, content).ConfigureAwait(false);
    }

    private static DataOutputFormat? TryGetFormatFromExtension(string? outputFile)
    {
        if (string.IsNullOrWhiteSpace(outputFile))
        {
            return null;
        }

        var extension = Path.GetExtension(outputFile);

        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        return extension.ToLowerInvariant() switch
        {
            ".json" => DataOutputFormat.Json,
            ".yaml" => DataOutputFormat.Yaml,
            ".yml" => DataOutputFormat.Yaml,
            _ => null
        };
    }
}
