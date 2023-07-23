using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Select;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Status;
using CreativeCoders.SysConsole.Cli.Actions;
using CreativeCoders.SysConsole.Cli.Actions.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic;

[UsedImplicitly]
[CliController()]
[CliController("connection")]
public class ConnectionController
{
    private readonly ICliCommandExecutor _commandExecutor;

    public ConnectionController(ICliCommandExecutor commandExecutor)
    {
        _commandExecutor = Ensure.NotNull(commandExecutor, nameof(commandExecutor));
    }
    
    [CliAction("select", HelpText = "Selects the connection to use")]
    public Task<CliActionResult> SelectConnectionAsync(SelectOptions options)
    {
        return _commandExecutor.ExecuteAsync<SelectCommand, SelectOptions>(options);
    }
    
    [CliAction("status", HelpText = "Show current connection status")]
    public Task<CliActionResult> ShowStatusAsync()
    {
        return _commandExecutor.ExecuteAsync<StatusCommand>();
    }
}