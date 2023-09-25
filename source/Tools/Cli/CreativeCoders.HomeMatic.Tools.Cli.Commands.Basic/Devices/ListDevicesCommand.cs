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
        _console.MarkupLine("List all devices for all CCUs");
        _console.WriteLine();
        
        var client = await _cliHomeMaticClientBuilder.BuildAsync()
            .ConfigureAwait(false);

        var devices = await client.GetDevicesAsync().ConfigureAwait(false);

        PrintDevices(devices);
        
        _console.WriteLine();
        
        return 0;
    }
    
    private void PrintDevices(IEnumerable<CcuDevice> devices)
    {
        var devicesTable = new Table()
            .Border(TableBorder.None)
            .AddColumn("Name", x => x.Padding(new Padding(3, 0)))
            .AddColumn("Address", x => x.Padding(new Padding(3, 0)))
            .AddColumn("CCU")
            .AddColumn("Type", x => x.Padding(new Padding(3, 0)));
        
        devices.ForEach(x =>
        {
            var nameColumn = new Markup($"[bold teal]{x.Name}[/]");
            var addressColumn = new Markup($"[bold]{x.Address}[/]");
            var ccuColumn = new Markup($"[bold yellow]{x.CcuSystem.Name}[/]");
            var deviceTypeColumn = new Markup($"{x.DeviceType}");
            
            devicesTable.AddRow(nameColumn, addressColumn, ccuColumn, deviceTypeColumn);
        });
        
        _console.Write(devicesTable);
    }
}
