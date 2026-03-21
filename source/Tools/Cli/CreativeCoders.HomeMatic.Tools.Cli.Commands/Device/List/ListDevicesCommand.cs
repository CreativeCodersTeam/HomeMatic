using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.Core.Text;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using CreativeCoders.SysConsole.Core;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.List;

[UsedImplicitly]
[CliCommand([DeviceCommandGroup.Name, "list"], Description = "List all devices for all CCUs")]
public class ListDevicesCommand(IAnsiConsole console, ICliHomeMaticClientBuilder cliHomeMaticClientBuilder)
    : ICliCommand<ListDevicesOptions>
{
    private readonly ICliHomeMaticClientBuilder _cliHomeMaticClientBuilder = Ensure.NotNull(cliHomeMaticClientBuilder);

    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private static bool FilterDevices(ICcuDevice device, ListDevicesOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.FilterPattern))
        {
            return true;
        }

        return PatternMatcher.MatchesPattern(device.Name, options.FilterPattern) ||
               PatternMatcher.MatchesPattern(device.Uri.Address, options.FilterPattern) ||
               PatternMatcher.MatchesPattern(device.DeviceType, options.FilterPattern) ||
               PatternMatcher.MatchesPattern(device.Uri.CcuHost, options.FilterPattern);
    }

    private void PrintDevices(IEnumerable<ICcuDevice> devices, ListDevicesOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.SortField))
        {
            devices = devices.OrderBy(x => GetSortValue(x, options.SortField)).ToArray();
        }

        _console.PrintTable(devices,
        [
            new TableColumnDef<ICcuDevice>(x => x.Name, "Name"),
            new TableColumnDef<ICcuDevice>(x => x.Uri.Address, "Address"),
            new TableColumnDef<ICcuDevice>(x => x.Uri.HostDisplayName, "CCU"),
            new TableColumnDef<ICcuDevice>(x => x.DeviceType, "Type")
        ]);
    }

    public async Task<CommandResult> ExecuteAsync(ListDevicesOptions options)
    {
        _console.MarkupLine("List all devices for all CCUs");
        _console.WriteLine();

        var multiCcuClient = await _cliHomeMaticClientBuilder.BuildMultiCcuClientAsync().ConfigureAwait(false);

        PrintDevices((await multiCcuClient.GetDevicesAsync().ConfigureAwait(false)).Where(device =>
            FilterDevices(device, options)), options);

        _console.WriteLine();

        return CommandResult.Success;
    }

    private static string GetSortValue(ICcuDevice device, string sortField)
    {
        return sortField.ToLower() switch
        {
            "name" => device.Name,
            "address" => device.Uri.Address,
            "ccu" => device.Uri.HostDisplayName,
            "type" => device.DeviceType,
            _ => device.Name
        };
    }
}
