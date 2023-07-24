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
        _console.MarkupLine("List devices");
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
            //.HideHeaders()
            .AddColumn("Address", x => x.Padding(new Padding(3, 0)))
            .AddColumn("Type", x => x.Padding(new Padding(3, 0)));
        
        devices.ForEach(x =>
        {
            var addressColumn = new Markup(x.Address);
            var typeColumn = new Markup(x.DeviceType);

            devicesTable.AddRow(addressColumn, typeColumn);
        });
        
        _console.Write(devicesTable);
    }
}