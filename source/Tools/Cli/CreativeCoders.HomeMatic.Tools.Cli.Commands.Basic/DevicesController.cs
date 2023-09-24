using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices;
using CreativeCoders.SysConsole.Cli.Actions;
using CreativeCoders.SysConsole.Cli.Actions.Definition;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic;

[UsedImplicitly]
[CliController("device")]
public class DevicesController
{
    private readonly ICliCommandExecutor _commandExecutor;

    public DevicesController(ICliCommandExecutor commandExecutor)
    {
        _commandExecutor = Ensure.NotNull(commandExecutor);
    }
    
    [CliAction("list")]
    public Task<CliActionResult> ListAsync(ListDevicesOptions options)
        => _commandExecutor.ExecuteAsync<ListDevicesCommand, ListDevicesOptions>(options);
}
