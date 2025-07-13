using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
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

        _console.MarkupLine($"Name:    [bold teal]{device.Name}[/]");
        _console.MarkupLine($"Address: [bold]{device.Uri.Address}[/]");
        _console.MarkupLine($"Ccu:     [bold yellow]{device.Uri.HostDisplayName}[/]");
        _console.MarkupLine($"Type:    {device.DeviceType}");

        _console.WriteLine();

        _console.WriteLine("Channels:");

        foreach (var channel in device.Channels)
        {
            _console.WriteLine($"  - Index:   {channel.Index}");
            _console.WriteLine($"    Address: {channel.Uri.Address}");
            _console.WriteLine($"    Type:    {channel.DeviceType}");
            _console.WriteLine("    ParamSets:");

            foreach (var paramSet in channel.ParamSets.Where(x => x != ParamSetKey.Link))
            {
                var values = await channel.GetParamSetValuesAsync(paramSet).ConfigureAwait(false);

                _console.WriteLine($"- {paramSet}");

                foreach (var paramSetValue in values)
                {
                    _console.WriteLine($"    - Name:  {paramSetValue.Name}");
                    _console.WriteLine($"      Value: {paramSetValue.Value}");
                }
            }
        }

        _console.WriteLine("Param sets:");

        foreach (var paramSet in device.ParamSets)
        {
            var values = await device.GetParamSetValuesAsync(paramSet).ConfigureAwait(false);

            _console.WriteLine($"- {paramSet}");

            foreach (var paramSetValue in values)
            {
                _console.WriteLine($"  - Name:  {paramSetValue.Name}");
                _console.WriteLine($"    Value: {paramSetValue.Value}");
            }
        }

        return 0;
    }
}
