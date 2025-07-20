using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.SysConsole.Cli.Actions.Definition;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic;

[CliController]
public class BasicController
{
    private readonly ICliCommandExecutor _commandExecutor;

    public BasicController(ICliCommandExecutor commandExecutor)
    {
        _commandExecutor = Ensure.NotNull(commandExecutor, nameof(commandExecutor));
    }
}
