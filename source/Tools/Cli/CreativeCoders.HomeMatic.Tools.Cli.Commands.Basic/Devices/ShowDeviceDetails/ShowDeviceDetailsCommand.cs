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
        var ccuClient = await _cliHomeMaticClientBuilder.BuildMultiCcuClientAsync();

        var device = await ccuClient.GetDeviceAsync(options.Address).ConfigureAwait(false);

        _console.WriteLine($"Show device details for '{options.Address}'");
        _console.WriteLine();

        await PrintDeviceAsync(device).ConfigureAwait(false);

        return 0;
    }

    private async Task PrintDeviceAsync(ICcuDevice device)
    {
        _console.MarkupLine($"Name:    [bold teal]{device.Name}[/]");
        _console.MarkupLine($"Address: [bold]{device.Uri.Address}[/]");
        _console.MarkupLine($"Ccu:     [bold yellow]{device.Uri.HostDisplayName}[/]");
        _console.MarkupLine($"Type:    {device.DeviceType}");

        _console.WriteLine();

        _console.WriteLine("Channels:");

        await device.Channels
            .ForEachAsync(PrintChannelAsync);

        await PrintParamSetsAsync(device, "  ");
    }

    private async Task PrintChannelAsync(ICcuDeviceChannel channel)
    {
        _console.WriteLine($"  - Index:   {channel.Index}");
        _console.WriteLine($"    Address: {channel.Uri.Address}");
        _console.WriteLine($"    Type:    {channel.DeviceType}");
        _console.WriteLine("    ParamSets:");

        await PrintParamSetsAsync(channel, "    ").ConfigureAwait(false);
    }

    private async Task PrintParamSetsAsync(ICcuDeviceBase deviceOrChannel, string indent)
    {
        foreach (var paramSet in deviceOrChannel.ParamSets.Where(x => x != ParamSetKey.Link))
        {
            var values = await deviceOrChannel.GetParamSetValuesAsync(paramSet).ConfigureAwait(false);

            _console.WriteLine($"{indent}- ParamSet: {paramSet}");

            foreach (var paramSetValue in values)
            {
                _console.WriteLine($"{indent}  - {paramSetValue.Name} : {paramSetValue.Value}");
            }
        }
    }
}
