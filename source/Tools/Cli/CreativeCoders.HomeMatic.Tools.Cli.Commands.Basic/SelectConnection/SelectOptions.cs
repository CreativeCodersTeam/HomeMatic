using CreativeCoders.SysConsole.Cli.Parsing;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Select;

public class SelectOptions
{
    [OptionValue(0)]
    public string CcuHost { get; set; } = string.Empty;
}