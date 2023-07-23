using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.SysConsole.Cli.Actions;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Select;

public class SelectCommand : IHomeMaticCliCommand<SelectOptions>
{
    private readonly IAnsiConsole _console;

    public SelectCommand(IAnsiConsole console)
    {
        _console = Ensure.NotNull(console, nameof(console));
    }
    
    public async Task<CliActionResult> ExecuteAsync(SelectOptions options)
    {
        _console.MarkupLine("Select CCU connection");
        _console.WriteLine();

        return new CliActionResult(0);
    }
}