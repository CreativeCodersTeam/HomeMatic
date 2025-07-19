using System.Text.Json;
using CreativeCoders.Core.IO;
using CreativeCoders.Core.Text;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Basic.Devices.ExportDevices;

public class ExportDevicesCommand(IAnsiConsole console, ICliHomeMaticClientBuilder cliHomeMaticClientBuilder)
    : IHomeMaticCliCommandWithOptions<ExportDevicesOptions>
{
    public async Task<int> ExecuteAsync(ExportDevicesOptions options)
    {
        var ccuClient = await cliHomeMaticClientBuilder.BuildMultiCcuClientAsync().ConfigureAwait(false);

        console.WriteLine("Export devices");

        if (string.IsNullOrWhiteSpace(options.Address))
        {
            console.WriteLine("No address specified");
            return -1;
        }

        var completeDevice = await ccuClient.GetCompleteDeviceAsync(options.Address).ConfigureAwait(false);

        var jsonData = new
        {
            Device = completeDevice.DeviceData,
            Channels = completeDevice.Channels,
            ParamSets = completeDevice.ParamSetValues
        };

        FileSys.File.WriteAllText(options.OutputFileName,
            jsonData.ToJson(new JsonSerializerOptions { WriteIndented = true }));

        return 0;
    }
}
