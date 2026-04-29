namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commands;

/// <summary>
/// Specifies the serialization format used to write CLI command output.
/// </summary>
public enum DataOutputFormat
{
    /// <summary>
    /// Format is derived from the output file extension; falls back to <see cref="Json"/>.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// JSON output.
    /// </summary>
    Json = 1,

    /// <summary>
    /// YAML output.
    /// </summary>
    Yaml = 2
}
