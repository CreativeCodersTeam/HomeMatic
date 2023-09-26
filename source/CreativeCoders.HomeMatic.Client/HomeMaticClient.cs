using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.Core.Threading;
using CreativeCoders.HomeMatic.Client.Core;
using CreativeCoders.HomeMatic.Client.Core.Devices;

namespace CreativeCoders.HomeMatic.Client;

public class HomeMaticClient : IHomeMaticClient
{
    private readonly HomeMaticCcuConnection[] _connections;

    public HomeMaticClient(HomeMaticCcuConnection[] connections)
    {
        _connections = Ensure.NotNull(connections);
    }

    public async Task<IEnumerable<CcuDevice>> GetDevicesAsync()
    {
        var deviceList = new ConcurrentList<CcuDevice>();

        await Parallel.ForEachAsync(_connections, async (connection, _) =>
        {
            var deviceDetails = await connection.JsonRpcClient
                .ListAllDetailsAsync()
                .ConfigureAwait(false);

            await connection.XmlRpcApis.ForEachAsync(async x =>
                {
                    var devices = await x.Api.ListDevicesAsync()
                        .ConfigureAwait(false);

                    deviceList.AddRange(devices.Where(x => x.IsDevice).Select(device =>
                        {
                            return new CcuDevice(new CcuSystemInfo(connection.Info.Name, x.DeviceSystem),
                                deviceDetails.FirstOrDefault(d => d.Address == device.Address)?.Name,
                                device);
                        }
                    ));
                })
                .ConfigureAwait(false);
        });

        return deviceList.OrderBy(x => x.Name);
    }
}
