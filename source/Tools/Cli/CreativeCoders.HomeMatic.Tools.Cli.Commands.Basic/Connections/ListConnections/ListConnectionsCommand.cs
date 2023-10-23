using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.ListConnections;

public class ListConnectionsCommand : IHomeMaticCliCommandWithOptions<ListConnectionsOptions>
{
    private readonly IAnsiConsole _console;
    
    private readonly ICcuConnectionsStore _ccuConnectionsStore;

    public ListConnectionsCommand(IAnsiConsole console, ICcuConnectionsStore ccuConnectionsStore)
    {
        _console = Ensure.NotNull(console);
        _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);
    }
    
    public async Task<int> ExecuteAsync(ListConnectionsOptions options)
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

        return 0;
    }
}
