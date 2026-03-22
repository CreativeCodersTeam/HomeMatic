using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection.Add;

[UsedImplicitly]
[CliCommand([ConnectionCommandGroup.Name, "add"], Description = "Add a new CCU connection")]
public class AddConnectionCommand(IAnsiConsole console, ICcuConnectionsStore ccuConnectionsStore)
    : ICliCommand<AddConnectionOptions>
{
    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private readonly ICcuConnectionsStore _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);

    public async Task<CommandResult> ExecuteAsync(AddConnectionOptions options)
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
        return CommandResult.Success;
    }
}
