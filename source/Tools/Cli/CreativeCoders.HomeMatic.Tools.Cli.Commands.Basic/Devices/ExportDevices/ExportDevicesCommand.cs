using CreativeCoders.HomeMatic.Abstractions.Devices;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ExportDevices;

[UsedImplicitly]
public class ExportDevicesCommand(IAnsiConsole console, ICliHomeMaticClientBuilder cliHomeMaticClientBuilder)
    : IHomeMaticCliCommandWithOptions<ExportDevicesOptions>
{
    public async Task<int> ExecuteAsync(ExportDevicesOptions options)
    {
        var ccuClient = await cliHomeMaticClientBuilder.BuildMultiCcuClientAsync().ConfigureAwait(false);

        console.WriteLine("Export devices");

        if (string.IsNullOrWhiteSpace(options.Address) || string.IsNullOrWhiteSpace(options.OutputFileName))
        {
            console.WriteLine("No address or output file specified");
            return -1;
        }

        await new JsonDataExporterBase<ICompleteCcuDevice>(() => ccuClient.GetCompleteDeviceAsync(options.Address),
                device => new
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
                }).ExportAsync(options.OutputFileName)
            .ConfigureAwait(false);

        // var completeDevice = await ccuClient.GetCompleteDeviceAsync(options.Address).ConfigureAwait(false);
        //
        // var jsonData = new
        // {
        //     Device = completeDevice.DeviceData,
        //     Channels = completeDevice.Channels.Select(x =>
        //     {
        //         var channel = new
        //         {
        //             Info = x.ChannelData,
        //             ParamSets = x.ParamSetValues
        //         };
        //
        //         return channel;
        //     }),
        //     ParamSets = completeDevice.ParamSetValues
        // };
        //
        // await FileSys.File.WriteAllTextAsync(options.OutputFileName,
        //         jsonData.ToJson(new JsonSerializerOptions { WriteIndented = true }))
        //     .ConfigureAwait(false);

        return 0;
    }
}
