using CreativeCoders.HomeMatic.Abstractions;

namespace CreativeCoders.HomeMatic;

public class MultiCcuClient(IEnumerable<ICcuClient> ccuClients) : IMultiCcuClient
{
    public async Task<IEnumerable<ICcuDevice>> GetDevicesAsync()
    {
        var deviceTasks = ccuClients.Select(client => client.GetDevicesAsync());

        var devicesPerClient = await Task.WhenAll(deviceTasks).ConfigureAwait(false);

        return devicesPerClient.SelectMany(devices => devices);
    }

    public async Task<ICcuDevice> GetDeviceAsync(string address)
    {
        return (await GetDevicesAsync().ConfigureAwait(false)).FirstOrDefault(x => x.Uri.Address == address) ??
               throw new KeyNotFoundException($"Device with address '{address}' not found.");
    }
}
