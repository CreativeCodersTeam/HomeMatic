using CreativeCoders.SysConsole.Cli.Actions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public interface IHomeMaticCliCommandWithOptions<TOptions>
    where TOptions : class
{
    Task<int> ExecuteAsync(TOptions options);
}