using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Status;

[UsedImplicitly]
public class StatusCommand : IHomeMaticCliCommand
{
    private readonly IAnsiConsole _console;
    
    private readonly ISharedData _sharedData;
    
    private readonly IHomeMaticXmlRpcApiBuilder _apiBuilder;

    public StatusCommand(IAnsiConsole console, ISharedData sharedData, IHomeMaticXmlRpcApiBuilder apiBuilder)
    {
        _console = Ensure.NotNull(console, nameof(console));
        _sharedData = Ensure.NotNull(sharedData, nameof(sharedData));
        _apiBuilder = Ensure.NotNull(apiBuilder, nameof(apiBuilder));
    }
    
    public async Task<int> ExecuteAsync()
    {
        var cliData = _sharedData.LoadCliData();
        
        _console.MarkupLine($"CCU host: [green]{cliData.CcuHost}[/]");
        
        var api = _apiBuilder
            .ForUrl($"http://{cliData.CcuHost}:{CcuRpcPorts.HomeMaticIp}")
            .Build();
        
        (await api.ListDevicesAsync()).ForEach(x => _console.MarkupLine($"{x.Address}"));
        
        return 0;
    }
}