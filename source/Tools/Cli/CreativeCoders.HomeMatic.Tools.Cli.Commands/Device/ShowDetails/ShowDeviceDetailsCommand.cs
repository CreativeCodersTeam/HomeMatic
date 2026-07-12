using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.Exporting;
using CreativeCoders.SysConsole.Core;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.ShowDetails;

[UsedImplicitly]
[CliCommand([DeviceCommandGroup.Name, "details"], Description = "Show details for a device")]
public class ShowDeviceDetailsCommand(
    IAnsiConsole console,
    IMultiCcuClient multiCcuClient,
    IDeviceExporter deviceExporter)
    : ICliCommand<ShowDeviceDetailsOptions>
{
    private readonly IMultiCcuClient _multiCcuClient = Ensure.NotNull(multiCcuClient);

    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private readonly IDeviceExporter _deviceExporter = Ensure.NotNull(deviceExporter);

    public async Task<CommandResult> ExecuteAsync(ShowDeviceDetailsOptions options)
    {
        var exportOptions = new DeviceExportOptions
        {
            IncludeLinks = true,
            SkipServiceParamSet = options.SkipServiceParamSet,
            ParamSetWhitelist = string.IsNullOrWhiteSpace(options.ParamSets)
                ? null
                : options.ParamSets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        };

        var device = await _multiCcuClient
            .GetCompleteDeviceAsync(options.Address, exportOptions.ToBuildOptions())
            .ConfigureAwait(false);

        var exportData = _deviceExporter.BuildExportData(device, exportOptions);

        _console.WriteLine($"Show device details for '{options.Address}'");
        _console.WriteLine();

        var serviceIsOverridden = exportOptions.SkipServiceParamSet
                                  && exportOptions.ParamSetWhitelist is not null
                                  && exportOptions.ParamSetWhitelist.Contains(ParamSetKey.Service,
                                      StringComparer.OrdinalIgnoreCase);

        if (serviceIsOverridden)
        {
            _console.MarkupLine("[yellow]--skip-service-params overrides SERVICE in --param-sets.[/]");
            _console.WriteLine();
        }

        // The export data no longer proves that the device offers ParamSets at all, because its key lists are
        // filtered as well. The unfiltered snapshot does.
        var deviceOffersParamSets = device.DeviceData.ParamSets.Length > 0
                                    || device.Channels.Any(x => x.ChannelData.ParamSets.Length > 0);

        // Does any whitelist entry still stand a chance after the skip removed SERVICE? If none does, the filter
        // did not fail to match - the skip emptied it, and the warning above already says so. Only a surviving
        // entry that matched nothing justifies blaming the --param-sets filter.
        var whitelistHasSurvivingEntry =
            exportOptions.ParamSetWhitelist?.Any(exportOptions.IsParamSetAllowed) == true;

        if (whitelistHasSurvivingEntry
            && !exportData.ParamSetValues.Any()
            && exportData.Channels.All(x => !x.ParamSetValues.Any())
            && deviceOffersParamSets)
        {
            _console.MarkupLine("[yellow]No ParamSets matched the --param-sets filter.[/]");
            _console.WriteLine();
        }

        PrintDevice(exportData);

        return CommandResult.Success;
    }

    private void PrintDevice(DeviceExportData device)
    {
        _console.MarkupLine($"Name:             [bold teal]{Markup.Escape(device.Name)}[/]");
        _console.MarkupLine($"Address:          [bold]{Markup.Escape(device.Address)}[/]");
        _console.MarkupLine($"Ccu:              [bold yellow]{Markup.Escape(device.Ccu)}[/]");
        _console.MarkupLine($"Type:             {Markup.Escape(device.DeviceType)}");
        _console.MarkupLine($"Firmware:         {Markup.Escape(device.FirmwareVersion)}");
        _console.MarkupLine($"ParamSet keys:    {Markup.Escape(string.Join(", ", device.ParamSetKeys))}");

        _console.WriteLine();
        _console.WriteLine("Device ParamSets:");
        PrintParamSets(device.ParamSetValues, "  ");

        _console.WriteLine();
        _console.WriteLine("Channels:");

        foreach (var channel in device.Channels)
        {
            PrintChannel(channel);
        }
    }

    private void PrintChannel(ChannelExportData channel)
    {
        _console.WriteLine($"  - Index:         {channel.Index}");
        _console.WriteLine($"    Address:       {channel.Address}");
        _console.WriteLine($"    Type:          {channel.DeviceType}");
        _console.WriteLine($"    ParamSet keys: {string.Join(", ", channel.ParamSets)}");

        _console.WriteLine("    Channel ParamSets:");
        PrintParamSets(channel.ParamSetValues, "      ");

        if (channel.Links is not null)
        {
            PrintLinks(channel.Links, "    ");
        }
    }

    private void PrintParamSets(IEnumerable<ParamSetExportData> paramSets, string indent)
    {
        foreach (var paramSet in paramSets)
        {
            _console.WriteLine($"{indent}- ParamSet: {paramSet.ParamSetKey}");

            if (paramSet.Error is not null)
            {
                _console.MarkupLine(
                    $"{indent}  [yellow]Values could not be read: {Markup.Escape(paramSet.Error)}[/]");

                continue;
            }

            foreach (var value in paramSet.Values)
            {
                var label = value.Name is not null && !string.Equals(value.Name, value.Key, StringComparison.Ordinal)
                    ? $"{value.Key} ({value.Name})"
                    : value.Key;

                _console.WriteLine($"{indent}  - {label} : {value.Value}");
            }
        }
    }

    private void PrintLinks(IEnumerable<LinkExportData> links, string indent)
    {
        var linkList = links.ToList();

        if (linkList.Count == 0)
        {
            return;
        }

        _console.WriteLine($"{indent}Links:");

        foreach (var link in linkList)
        {
            _console.WriteLine($"{indent}  - Sender:      {link.Sender}");
            _console.WriteLine($"{indent}    Receiver:    {link.Receiver}");
            _console.WriteLine($"{indent}    Name:        {link.Name}");
            _console.WriteLine($"{indent}    Description: {link.Description}");
        }
    }
}
