using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Backup;

[PublicAPI]
public class BackupCcuOptions
{
    [OptionValue(0, IsRequired = true, HelpText = "Name of the configured CCU connection")]
    public string Name { get; set; } = string.Empty;

    [OptionParameter('o', "output", IsRequired = true,
        HelpText = "Path of the backup file to create")]
    public string OutputFile { get; set; } = string.Empty;
}
