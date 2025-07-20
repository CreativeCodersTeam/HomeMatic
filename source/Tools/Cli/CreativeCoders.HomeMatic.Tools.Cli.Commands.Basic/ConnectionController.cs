using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.AddConnection;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.ListConnections;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Connections.RemoveConnection;
using CreativeCoders.SysConsole.Cli.Actions;
using CreativeCoders.SysConsole.Cli.Actions.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic;

[UsedImplicitly]
[CliController("connection")]
public class ConnectionController(ICliCommandExecutor commandExecutor)
{
    private readonly ICliCommandExecutor _commandExecutor = Ensure.NotNull(commandExecutor);

    [CliAction("add", HelpText = "Adds a new CCU connection to the available connections")]
    public Task<CliActionResult> AddAsync(AddConnectionOptions options)
        => _commandExecutor.ExecuteAsync<AddConnectionCommand, AddConnectionOptions>(options);

    [CliAction("remove", HelpText = "Removes a CCU connection from the available connections")]
    public Task<CliActionResult> RemoveAsync(RemoveConnectionOptions options)
        => _commandExecutor.ExecuteAsync<RemoveConnectionCommand, RemoveConnectionOptions>(options);

    [CliAction("list", HelpText = "Lists all available CCU connections")]
    public Task<CliActionResult> ListAsync(ListConnectionsOptions options)
        => _commandExecutor.ExecuteAsync<ListConnectionsCommand, ListConnectionsOptions>(options);
}
