using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Exporting;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Device.Export;

[UsedImplicitly]
[CliCommand([DeviceCommandGroup.Name, "export"], Description = "Export device to json file")]
public class ExportDevicesCommand(IAnsiConsole console, IMultiCcuClient multiCcuClient, IDeviceExporter deviceExporter)
    : JsonExportCommandBase<ICompleteCcuDevice, ExportDevicesOptions>(console, multiCcuClient)
{
    private readonly IDeviceExporter _deviceExporter = Ensure.NotNull(deviceExporter);

    protected override object TransformData(ICompleteCcuDevice device)
    {
        return _deviceExporter.BuildExportData(device, new DeviceExportOptions
        {
            WriteIndented = true
        });
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

    protected override string GetOutputFileName(ExportDevicesOptions options)
    {
        return options.OutputFileName;
    }
}
