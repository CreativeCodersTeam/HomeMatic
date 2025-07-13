using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
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
        var client = await _cliHomeMaticClientBuilder.BuildAsync();
        var ccuClient = await _cliHomeMaticClientBuilder.BuildMultiCcuClientAsync();

        //var device = await client.GetDeviceDescriptionAsync(options.Address);
        var device = await ccuClient.GetDeviceAsync(options.Address).ConfigureAwait(false);

        _console.MarkupLine($"[bold teal]{device.Name}[/]");
        _console.MarkupLine($"[bold]{device.Uri.Address}[/]");
        _console.MarkupLine($"[bold yellow]{device.Uri.HostDisplayName}[/]");
        _console.MarkupLine($"{device.DeviceType}");

        _console.WriteLine();

        _console.WriteLine("Param sets:");

        foreach (var paramSet in device.ParamSets)
        {
            _console.WriteLine($"- {paramSet}");
            // var paramSetValues = await device.GetParamSetAsync(paramSet).ConfigureAwait(false);
            //
            // foreach (var paramSetValue in paramSetValues)
            // {
            //     _console.WriteLine($"    {paramSetValue.Key}: {paramSetValue.Value}");
            // }
        }

        device.ParamSets.ForEach(x => { });

        return 0;
    }
}
