using AutoMapper.Internal;
using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Abstractions.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ShowDeviceDetails;

[UsedImplicitly]
public class ShowDeviceDetailsCommand : IHomeMaticCliCommandWithOptions<ShowDeviceDetailsOptions>
{
    private readonly ICliHomeMaticClientBuilder _cliHomeMaticClientBuilder;

    private readonly IAnsiConsole _console;

    public ShowDeviceDetailsCommand(IAnsiConsole console, ICliHomeMaticClientBuilder cliHomeMaticClientBuilder)
    {
        _console = Ensure.NotNull(console);
        _cliHomeMaticClientBuilder = Ensure.NotNull(cliHomeMaticClientBuilder);
    }

    public async Task<int> ExecuteAsync(ShowDeviceDetailsOptions options)
    {
        var ccuClient = await _cliHomeMaticClientBuilder.BuildMultiCcuClientAsync().ConfigureAwait(false);

        var device = await ccuClient.GetCompleteDeviceAsync(options.Address).ConfigureAwait(false);

        device.GetType().IsDynamic();
        _console.WriteLine($"Show device details for '{options.Address}'");
        _console.WriteLine();

        PrintDevice(device);

        return 0;
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

            foreach (var paramSetValue in values)
            {
                _console.WriteLine(
                    $"{indent}  - {paramSetValue.ParamSetValue.Name} : {paramSetValue.ParamSetValue.Value}");
            }
        }
    }
}
