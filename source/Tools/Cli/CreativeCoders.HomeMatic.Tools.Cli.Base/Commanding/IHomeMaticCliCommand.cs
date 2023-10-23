using CreativeCoders.SysConsole.Cli.Actions;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

public interface IHomeMaticCliCommand
{
    Task<int> ExecuteAsync();
}