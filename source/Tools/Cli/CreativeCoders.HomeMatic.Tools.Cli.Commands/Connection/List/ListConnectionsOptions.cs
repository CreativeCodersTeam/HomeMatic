using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection.List;

[UsedImplicitly]
public class ListConnectionsOptions
{
    [OptionParameter('l', "listview", HelpText = "Show detailed output as listview")]
    public bool ShowAsListView { get; set; }
}
