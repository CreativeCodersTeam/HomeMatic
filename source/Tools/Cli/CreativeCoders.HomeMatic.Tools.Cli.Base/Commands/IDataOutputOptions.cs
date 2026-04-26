namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands;

/// <summary>
/// Contract for CLI command options that control serialized data output.
/// </summary>
public interface IDataOutputOptions
{
    /// <summary>
    /// Gets the desired output format. When set to <see cref="DataOutputFormat.Auto"/>
    /// the format is derived from the output file extension.
    /// </summary>
    DataOutputFormat OutputFormat { get; }

    /// <summary>
    /// Gets the path of the output file. When <c>null</c> or empty the data is written to stdout.
    /// </summary>
    string? OutputFile { get; }
}
