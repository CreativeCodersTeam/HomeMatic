using CreativeCoders.SysConsole.Cli.Parsing;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public class CliCommandOptionsBase
{
    [OptionParameter('v', "verbose", HelpText = "Verbose output")]
    public CliCommandVerbosity Verbosity { get; set; }
}

public enum CliCommandVerbosity
{
    Quiet,
    Normal,
    Verbose,
    Debug
}
