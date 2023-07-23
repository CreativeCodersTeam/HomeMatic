using CreativeCoders.Core;
using CreativeCoders.Core.Reflection;
using CreativeCoders.SysConsole.Cli.Actions;
using CreativeCoders.SysConsole.Cli.Actions.Exceptions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public class CliCommandExecutor : ICliCommandExecutor
{
    private readonly IServiceProvider _serviceProvider;

    public CliCommandExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = Ensure.NotNull(serviceProvider, nameof(serviceProvider));
    }

    public Task<CliActionResult> ExecuteAsync<TCommand, TOptions>(TOptions options)
        where TCommand : IHomeMaticCliCommand<TOptions> where TOptions : class
    {
        var command = typeof(TCommand).CreateInstance<IHomeMaticCliCommand<TOptions>>(_serviceProvider);

        return command != null
            ? command.ExecuteAsync(options)
            : throw new CliActionException("Command object cannot be created");
    }
}