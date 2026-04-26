using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Backup;

/// <summary>
/// Options for the CCU backup command.
/// </summary>
[UsedImplicitly]
public class CreateBackupOptions
{
    /// <summary>
    /// Gets or sets the name of the CCU connection to back up.
    /// </summary>
    [OptionValue(0, IsRequired = true)]
    public string ConnectionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the output file path for the backup archive. When not specified, a default file name is generated.
    /// </summary>
    [OptionParameter('o', "output", HelpText = "Output file path for the backup archive (.tar.gz)")]
    public string? OutputFilePath { get; set; }
}
