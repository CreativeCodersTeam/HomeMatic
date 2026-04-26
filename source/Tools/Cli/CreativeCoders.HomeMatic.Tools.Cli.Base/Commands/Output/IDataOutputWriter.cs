namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands.Output;

/// <summary>
/// Resolves the effective output format and writes serialized data either to a file or to stdout.
/// </summary>
public interface IDataOutputWriter
{
    /// <summary>
    /// Determines the effective <see cref="DataOutputFormat"/> based on the requested format
    /// and the output file extension.
    /// </summary>
    /// <param name="requestedFormat">The format requested via options.</param>
    /// <param name="outputFile">The path of the output file, or <c>null</c>/empty for stdout.</param>
    /// <returns>The resolved, concrete output format.</returns>
    DataOutputFormat ResolveFormat(DataOutputFormat requestedFormat, string? outputFile);

    /// <summary>
    /// Writes the serialized <paramref name="content"/> to the configured target.
    /// </summary>
    /// <param name="content">The already serialized content.</param>
    /// <param name="outputFile">The target file path. When <c>null</c> or empty, the content is written to stdout.</param>
    /// <returns>A task that completes when the content has been written.</returns>
    Task WriteAsync(string content, string? outputFile);
}
