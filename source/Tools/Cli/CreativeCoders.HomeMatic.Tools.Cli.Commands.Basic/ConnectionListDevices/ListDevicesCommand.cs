using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.ConnectionListDevices;

public class ListDevicesCommand : CliBaseCommand, IHomeMaticCliCommandWithOptions<ListDevicesOptions>
{
    private readonly IAnsiConsole _console;

    public ListDevicesCommand(IAnsiConsole console, IHomeMaticXmlRpcApiBuilder apiBuilder, ISharedData sharedData)
        : base(apiBuilder, sharedData)
    {
        _console = Ensure.NotNull(console, nameof(console));
    }
    
    public async Task<int> ExecuteAsync(ListDevicesOptions options)
    {
        var cliData = SharedData.LoadCliData();
        
        _console.MarkupLine($"List devices for CCU : [green]{cliData.CcuHost}[/]");
        _console.WriteLine();
        
        var api = BuildApi();

        var devices = (await api.ListDevicesAsync().ConfigureAwait(false))
            .Where(x => x.IsDevice)
            .OrderBy(x => x.DeviceType);
        
        PrintDevices(devices);

        return 0;
    }

    private void PrintDevices(IOrderedEnumerable<DeviceDescription> devices)
    {
        var devicesTable = new Table()
            .Border(TableBorder.None)
            .AddColumn("Address", x => x.Padding(new Padding(3, 0)))
            .AddColumn("Type", x => x.Padding(new Padding(3, 0)))
            .AddColumn("Parameter Sets", x => x.Padding(new Padding(3, 0)));
        
        devices.ForEach(x =>
        {
            var addressColumn = new Markup($"[bold]{x.Address}[/]");
            var typeColumn = new Markup($"[bold teal]{x.DeviceType}[/]");
            var interfaceColumn = new Markup($"[bold gray]{string.Join(", ", x.ParamSets)}[/]");

            devicesTable.AddRow(addressColumn, typeColumn, interfaceColumn);
        });
        
        _console.Write(devicesTable);
    }
}