using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Abstractions.Devices;

namespace CreativeCoders.HomeMatic;

public class MultiCcuClient(IEnumerable<ICcuClient> ccuClients) : IMultiCcuClient
{
    public Task<IEnumerable<ICcuDevice>> GetDevicesAsync()
    {
        return GetDataFromClientsAsync(x => x.GetDevicesAsync());
        // var deviceTasks = ccuClients.Select(client => client.GetDevicesAsync());
        //
        // var devicesPerClient = await Task.WhenAll(deviceTasks).ConfigureAwait(false);
        //
        // return devicesPerClient.SelectMany(devices => devices);
    }

    public async Task<ICcuDevice> GetDeviceAsync(string address)
    {
        return (await GetDevicesAsync().ConfigureAwait(false)).FirstOrDefault(x => x.Uri.Address == address) ??
               throw new KeyNotFoundException($"Device with address '{address}' not found.");
    }

    public Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDeviceAsync(string address)
    {
        throw new NotImplementedException();
    }

    private async Task<IEnumerable<T>> GetDataFromClientsAsync<T>(Func<ICcuClient, Task<IEnumerable<T>>> func)
    {
        var tasks = ccuClients.Select(func);

        var dataFromClients = await Task.WhenAll(tasks).ConfigureAwait(false);

        return dataFromClients.SelectMany(x => x);
    }
}
