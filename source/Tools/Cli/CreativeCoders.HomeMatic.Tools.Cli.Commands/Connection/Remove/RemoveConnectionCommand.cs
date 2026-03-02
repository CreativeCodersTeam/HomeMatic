using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Connection.Remove;

[UsedImplicitly]
[CliCommand([ConnectionCommandGroup.Name, "remove"], Description = "Remove a CCU connection")]
public class RemoveConnectionCommand(IAnsiConsole console, ICcuConnectionsStore ccuConnectionsStore)
    : ICliCommand<RemoveConnectionOptions>
{
    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private readonly ICcuConnectionsStore _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);

    public async Task<CommandResult> ExecuteAsync(RemoveConnectionOptions options)
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
        return CommandResult.Success;
    }
}
