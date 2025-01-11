using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.Core.Text;
using CreativeCoders.HomeMatic.Client.Core.Devices;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ListDevices;

[UsedImplicitly]
public class ListDevicesCommand : IHomeMaticCliCommandWithOptions<ListDevicesOptions>
{
    private readonly ICliHomeMaticClientBuilder _cliHomeMaticClientBuilder;

    private readonly IAnsiConsole _console;

    public ListDevicesCommand(IAnsiConsole console, ICliHomeMaticClientBuilder cliHomeMaticClientBuilder)
    {
        _console = Ensure.NotNull(console);
        _cliHomeMaticClientBuilder = Ensure.NotNull(cliHomeMaticClientBuilder);
    }

    private static bool FilterDevices(ICcuDevice device, ListDevicesOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.FilterPattern))
        {
            return true;
        }

        return PatternMatcher.MatchesPattern(device.Name, options.FilterPattern) ||
               PatternMatcher.MatchesPattern(device.Address, options.FilterPattern) ||
               PatternMatcher.MatchesPattern(device.DeviceType, options.FilterPattern) ||
               PatternMatcher.MatchesPattern(device.CcuSystem.Name, options.FilterPattern);
    }

    private void PrintDevices(IEnumerable<ICcuDevice> devices)
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

    public async Task<int> ExecuteAsync(ListDevicesOptions options)
    {
        _console.MarkupLine("List all devices for all CCUs");
        _console.WriteLine();

        var client = await _cliHomeMaticClientBuilder.BuildAsync()
            .ConfigureAwait(false);

        var devices = await client.GetDevicesAsync().ConfigureAwait(false);

        PrintDevices(devices.Where(x => FilterDevices(x, options)));

        _console.WriteLine();

        return 0;
    }
}
