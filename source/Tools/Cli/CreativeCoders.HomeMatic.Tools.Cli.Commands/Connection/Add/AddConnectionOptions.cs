using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection.Add;

[UsedImplicitly]
public class AddConnectionOptions
{
    [OptionValue(0, IsRequired = true)]
    public string? Url { get; set; }

    [OptionParameter('n', "name", HelpText = "Name of the connection")]
    public string? Name { get; set; }
}
