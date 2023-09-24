using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.RemoveConnection;

public class RemoveConnectionCommand : IHomeMaticCliCommandWithOptions<RemoveConnectionOptions>
{
    private readonly IAnsiConsole _console;
    
    private readonly ICcuConnectionsStore _ccuConnectionsStore;

    public RemoveConnectionCommand(IAnsiConsole console, ICcuConnectionsStore ccuConnectionsStore)
    {
        _console = Ensure.NotNull(console);
        _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);
    }
    
    public async Task<int> ExecuteAsync(RemoveConnectionOptions options)
    {
        var removed = false;
        
        if (options.Url is not null)
        {
            removed = await _ccuConnectionsStore
                .RemoveConnectionAsync(new Uri(options.Url))
                .ConfigureAwait(false);
        }
        else
        {
            if (options.Name is null)
            {
                return -1;
            }
            
            removed = await _ccuConnectionsStore
                .RemoveConnectionAsync(options.Name)
                .ConfigureAwait(false);
        }

        if (!removed)
        {
            _console.MarkupLine("[bold italic red3]Connection not found[/]");
            return -1;
        }

        _console.MarkupLine("[bold lime]Connection removed[/]");
        return 0;
    }
}
