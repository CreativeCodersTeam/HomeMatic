using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection.List;

[UsedImplicitly]
[CliCommand([ConnectionCommandGroup.Name, "list"], Description = "List all available CCU connections")]
public class ListConnectionsCommand(IAnsiConsole console, ICcuConnectionsStore ccuConnectionsStore)
    : ICliCommand<ListConnectionsOptions>
{
    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private readonly ICcuConnectionsStore _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);

    public async Task<CommandResult> ExecuteAsync(ListConnectionsOptions options)
    {
        _console.MarkupLine("List all available CCU connections:");
        _console.WriteLine();

        var connectionsTable = new Table()
            .Border(TableBorder.None)
            .AddColumn("Name")
            .AddColumn("Url");

        var connections = await _ccuConnectionsStore.GetConnectionsAsync().ConfigureAwait(false);

        connections
            .ForEach(x => connectionsTable.AddRow(x.Name, x.Url.ToString()));

        _console.Write(connectionsTable);

        _console.WriteLine();

        return CommandResult.Success;
    }
}
