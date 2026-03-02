using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection.Remove;

[UsedImplicitly]
public class RemoveConnectionOptions
{
    [OptionValue(0)]
    public string? Url { get; set; }

    [OptionParameter('n', "name", HelpText = "Name of the connection")]
    public string? Name { get; set; }
}
