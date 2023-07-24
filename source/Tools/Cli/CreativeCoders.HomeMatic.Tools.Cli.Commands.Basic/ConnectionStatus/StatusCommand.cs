using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.ConnectionStatus;

[UsedImplicitly]
public class StatusCommand : CliBaseCommand, IHomeMaticCliCommand
{
    private readonly IAnsiConsole _console;

    public StatusCommand(IAnsiConsole console, ISharedData sharedData, IHomeMaticXmlRpcApiBuilder apiBuilder)
        : base(apiBuilder, sharedData)
    {
        _console = Ensure.NotNull(console, nameof(console));
    }
    
    public async Task<int> ExecuteAsync()
    {
        var cliData = SharedData.LoadCliData();
        
        _console.MarkupLine($"CCU host: [green]{cliData.CcuHost}[/]");

        var api = BuildApi();

        var version = await api.GetVersionAsync().ConfigureAwait(false);
        
        _console.MarkupLine($"BidCos version: {version}");
        
        return 0;
    }
}
