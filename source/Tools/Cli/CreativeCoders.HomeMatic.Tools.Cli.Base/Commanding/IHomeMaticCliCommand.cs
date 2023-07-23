using CreativeCoders.SysConsole.Cli.Actions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public interface IHomeMaticCliCommand<TOptions>
    where TOptions : class
{
    Task<CliActionResult> ExecuteAsync(TOptions options);
}