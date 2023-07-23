using CreativeCoders.SysConsole.Cli.Actions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public interface ICliCommandExecutor
{
    Task<CliActionResult> ExecuteAsync<TCommand, TOptions>(TOptions options)
        where TCommand : IHomeMaticCliCommandWithOptions<TOptions>
        where TOptions : class;
    
    Task<CliActionResult> ExecuteAsync<TCommand>()
        where TCommand : IHomeMaticCliCommand;
}