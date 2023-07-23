using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.SysConsole.Cli.Actions;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Select;

[UsedImplicitly]
public class SelectCommand : IHomeMaticCliCommandWithOptions<SelectOptions>
{
    private readonly IAnsiConsole _console;
    
    private readonly ISharedData _sharedData;

    public SelectCommand(IAnsiConsole console, ISharedData sharedData)
    {
        _console = Ensure.NotNull(console, nameof(console));
        _sharedData = Ensure.NotNull(sharedData, nameof(sharedData));
    }
    
    public async Task<int> ExecuteAsync(SelectOptions options)
    {
        _console.MarkupLine("Select CCU connection");
        _console.WriteLine();

        var cliData = _sharedData.LoadCliData();

        cliData.CcuHost = options.CcuHost;
        
        _sharedData.SaveCliData(cliData);
        
        _console.MarkupLine($"Set CCU host to [green]{options.CcuHost}[/]");
        _console.WriteLine();

        return 0;
    }
}