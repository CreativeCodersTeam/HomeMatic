using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.SysConsole.Core;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.ShowDetails;

[UsedImplicitly]
[CliCommand([DeviceCommandGroup.Name, "details"], Description = "Show details for a device")]
public class ShowDeviceDetailsCommand(IAnsiConsole console, IMultiCcuClient multiCcuClient)
    : ICliCommand<ShowDeviceDetailsOptions>
{
    private readonly IMultiCcuClient _multiCcuClient = Ensure.NotNull(multiCcuClient);

    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    public async Task<CommandResult> ExecuteAsync(ShowDeviceDetailsOptions options)
    {
        var device = await _multiCcuClient.GetCompleteDeviceAsync(options.Address).ConfigureAwait(false);

        _console.WriteLine($"Show device details for '{options.Address}'");
        _console.WriteLine();

        PrintDevice(device);

        return CommandResult.Success;
    }

    private void PrintDevice(ICompleteCcuDevice device)
    {
        _console.MarkupLine($"Name:    [bold teal]{device.DeviceData.Name}[/]");
        _console.MarkupLine($"Address: [bold]{device.DeviceData.Uri.Address}[/]");
        _console.MarkupLine($"Ccu:     [bold yellow]{device.DeviceData.Uri.HostDisplayName}[/]");
        _console.MarkupLine($"Type:    {device.DeviceData.DeviceType}");

        _console.WriteLine();

        _console.WriteLine("  Device ParamSets:");

        _console.WriteLine("Channels:");

        device.Channels.ForEach(PrintChannel);

        PrintParamSets(device.ParamSetValues, "  ");
    }

    private void PrintChannel(ICompleteCcuDeviceChannel channel)
    {
        _console.WriteLine($"  - Index:   {channel.ChannelData.Index}");
        _console.WriteLine($"    Address: {channel.ChannelData.Uri.Address}");
        _console.WriteLine($"    Type:    {channel.ChannelData.DeviceType}");
        _console.WriteLine("    Channel ParamSets:");

        PrintParamSets(channel.ParamSetValues, "    ");
    }

    private void PrintParamSets(IEnumerable<ParamSetValuesWithDescriptions> paramSetValuesWithDescriptions,
        string indent)
    {
        foreach (var paramSet in paramSetValuesWithDescriptions)
        {
            var values = paramSet.ParamSetValues;

            _console.WriteLine($"{indent}- ParamSet: {paramSet.ParamSetKey}");

            _console.WriteLines(values.Select(x => $"{indent}  - {x.ParamSetValue.Name} : {x.ParamSetValue.Value}")
                .ToArray());
        }
    }
}
