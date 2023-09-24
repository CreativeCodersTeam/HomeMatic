using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.AddConnection;

[UsedImplicitly]
public class AddConnectionCommand : IHomeMaticCliCommandWithOptions<AddConnectionOptions>
{
    private readonly IAnsiConsole _console;
    
    private readonly ICcuConnectionsStore _ccuConnectionsStore;

    public AddConnectionCommand(IAnsiConsole console, ICcuConnectionsStore ccuConnectionsStore)
    {
        _console = Ensure.NotNull(console);
        _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);
    }
    
    public async Task<int> ExecuteAsync(AddConnectionOptions options)
    {
        if (options.Url is null)
        {
            return -1;
        }

        var url = new Uri(options.Url);

        var added = await _ccuConnectionsStore
            .AddConnectionAsync(new CcuConnectionInfo(url, options.Name))
            .ConfigureAwait(false);

        if (!added)
        {
            _console.MarkupLine("[bold italic red3]Connection already exists[/]");
            return -1;
        }
        
        _console.MarkupLine("[bold lime]Connection added[/]");
        return 0;
    }
}
