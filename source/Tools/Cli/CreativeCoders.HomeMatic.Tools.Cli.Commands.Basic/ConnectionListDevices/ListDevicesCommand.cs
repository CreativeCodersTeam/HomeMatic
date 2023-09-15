using System.Net;
using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.SharedData;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.ConnectionListDevices;

public class ListDevicesCommand : CliBaseCommand, IHomeMaticCliCommandWithOptions<ListDevicesOptions>
{
    private readonly IAnsiConsole _console;
    
    private readonly IHomeMaticJsonRpcApi _homeMaticJsonRpcApi;

    private readonly IHomeMaticJsonRpcApiBuilder _jsonRpcApiBuilder;

    public ListDevicesCommand(IAnsiConsole console, IHomeMaticXmlRpcApiBuilder apiBuilder,
        ISharedData sharedData, IHomeMaticJsonRpcApi homeMaticJsonRpcApi, IHomeMaticJsonRpcApiBuilder jsonRpcApiBuilder)
        : base(apiBuilder, sharedData)
    {
        _console = Ensure.NotNull(console, nameof(console));
        _homeMaticJsonRpcApi = Ensure.NotNull(homeMaticJsonRpcApi, nameof(_homeMaticJsonRpcApi));
        _jsonRpcApiBuilder = Ensure.NotNull(jsonRpcApiBuilder, nameof(jsonRpcApiBuilder));
    }
    
    public async Task<int> ExecuteAsync(ListDevicesOptions options)
    {
        var cliData = SharedData.LoadCliData();
        
        _console.MarkupLine($"List devices for CCU : [green]{cliData.CcuHost}[/]");
        _console.WriteLine();
        
        var api = BuildApi();
        
        _homeMaticJsonRpcApi.CcuHost = cliData.CcuHost;
        _homeMaticJsonRpcApi.Credentials =
            new NetworkCredential(cliData.Users.Values.First(), SharedData.GetPassword(cliData.CcuHost));

        var devices = (await api.ListDevicesAsync().ConfigureAwait(false))
            .Where(x => x.IsDevice)
            .OrderBy(x => x.DeviceType);
        
        var deviceDetails = await _homeMaticJsonRpcApi.ListAllDetailsAsync().ConfigureAwait(false);
        
        PrintDevices(devices, deviceDetails.Result);

        return 0;
    }

    private void PrintDevices(IOrderedEnumerable<DeviceDescription> devices,
        IEnumerable<DeviceDetails>? deviceDetailsEnumerable)
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
            var interfaceColumn = new Markup($"[bold gray]{string.Join(", ", x.ParamSets)}[/]");
            
            var detail = deviceDetailsEnumerable?.FirstOrDefault(y => y.Address == x.Address);

            var detailColumn = detail != null
                ? new Markup($"[bold]{detail.Name}[/]")
                : new Markup("[bold red]Unknown[/]");

            devicesTable.AddRow(addressColumn, detailColumn, typeColumn, interfaceColumn);
        });
        
        _console.Write(devicesTable);
    }
}