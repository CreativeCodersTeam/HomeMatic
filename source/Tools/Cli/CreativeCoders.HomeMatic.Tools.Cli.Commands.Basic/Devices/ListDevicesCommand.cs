using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Client.Core.Devices;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices;

[UsedImplicitly]
public class ListDevicesCommand : IHomeMaticCliCommandWithOptions<ListDevicesOptions>
{
    private readonly IAnsiConsole _console;
    
    private readonly ICliHomeMaticClientBuilder _cliHomeMaticClientBuilder;

    public ListDevicesCommand(IAnsiConsole console, ICliHomeMaticClientBuilder cliHomeMaticClientBuilder)
    {
        _console = Ensure.NotNull(console);
        _cliHomeMaticClientBuilder = Ensure.NotNull(cliHomeMaticClientBuilder);
    }
    
    public async Task<int> ExecuteAsync(ListDevicesOptions options)
    {
        var client = await _cliHomeMaticClientBuilder.BuildAsync()
            .ConfigureAwait(false);

        var devices = await client.GetDevicesAsync().ConfigureAwait(false);

        PrintDevices(devices);
        
        return 0;
    }
    
    private void PrintDevices(IEnumerable<CcuDevice> devices)
    {
        var devicesTable = new Table()
            .Border(TableBorder.None)
            .AddColumn("Address", x => x.Padding(new Padding(3, 0)))
            .AddColumn("Name", x => x.Padding(new Padding(3, 0)))
            .AddColumn("Type", x => x.Padding(new Padding(3, 0)))
            .AddColumn("Parameter Sets", x => x.Padding(new Padding(3, 0)));
        
        devices.ForEach(x =>
        {
            var addressColumn = new Markup($"[bold]{x.Address}[/]");
            var typeColumn = new Markup($"[bold teal]{x.DeviceType}[/]");
            //var interfaceColumn = new Markup($"[bold gray]{string.Join(", ", x.ParamSets)}[/]");
            
            var detailColumn = new Markup($"[bold]{x.Name}[/]");

            devicesTable.AddRow(addressColumn, detailColumn, typeColumn);
        });
        
        _console.Write(devicesTable);
    }
}
