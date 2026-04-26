using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Backup;

[UsedImplicitly]
public class BackupCcuOptions
{
    [OptionValue(0, IsRequired = true, HelpText = "Name of the configured CCU connection")]
    public string Name { get; set; } = string.Empty;

    [OptionParameter('o', "output", HelpText = "Output directory for the backup file (default: current directory)")]
    public string OutputDirectory { get; set; } = string.Empty;
}
