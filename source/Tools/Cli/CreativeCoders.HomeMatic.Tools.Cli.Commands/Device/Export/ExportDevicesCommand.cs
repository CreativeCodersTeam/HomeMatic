using CreativeCoders.Cli.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.Export;

[UsedImplicitly]
[CliCommand([DeviceCommandGroup.Name, "export"], Description = "Export device to json file")]
public class ExportDevicesCommand(IAnsiConsole console, ICliHomeMaticClientBuilder cliHomeMaticClientBuilder)
    : JsonExportCommandBase<ICompleteCcuDevice, ExportDevicesOptions>(console, cliHomeMaticClientBuilder)
{
    protected override object TransformData(ICompleteCcuDevice device)
    {
        return new
        {
            Device = device.DeviceData,
            Channels = device.Channels.Select(x =>
            {
                var channel = new
                {
                    Info = x.ChannelData,
                    ParamSets = x.ParamSetValues
                };

                return channel;
            }),
            ParamSets = device.ParamSetValues
        };
    }

    protected override Task<ICompleteCcuDevice> LoadDataAsync(IMultiCcuClient ccuClient, ExportDevicesOptions options)
    {
        return ccuClient.GetCompleteDeviceAsync(options.Address);
    }

    protected override bool ValidateOptions(ExportDevicesOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.Address) && !string.IsNullOrWhiteSpace(options.OutputFileName);
    }

    protected override string GetOutputFileName(ExportDevicesOptions options)
    {
        return options.OutputFileName;
    }
}
