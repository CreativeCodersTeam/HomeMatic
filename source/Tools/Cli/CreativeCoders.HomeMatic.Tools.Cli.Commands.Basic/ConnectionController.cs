using System.Runtime.InteropServices.ComTypes;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
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
    public Task<CliActionResult> SelectConnectionAsync()
    {
        return Task.FromResult(new CliActionResult(0));
    }
    
    [CliAction("status", HelpText = "Show current connection status")]
    public Task<CliActionResult> ShowStatusAsync()
    {
        return Task.FromResult(new CliActionResult(0));
    }
}