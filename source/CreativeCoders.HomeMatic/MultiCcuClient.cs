using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic;

public class MultiCcuClient(
    IEnumerable<ICcuClient> ccuClients) : IMultiCcuClient
{
    public Task<IEnumerable<ICcuDevice>> GetDevicesAsync()
    {
        return GetDataFromClientsAsync(x => x.GetDevicesAsync());
    }

    public async Task<ICcuDevice> GetDeviceAsync(string address)
    {
        return (await GetDevicesAsync().ConfigureAwait(false)).FirstOrDefault(x => x.Uri.Address == address) ??
               throw new KeyNotFoundException($"Device with address '{address}' not found.");
    }

    public Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync()
    {
        return GetDataFromClientsAsync(x => x.GetCompleteDevicesAsync());
    }

    public async Task<ICompleteCcuDevice> GetCompleteDeviceAsync(string address)
    {
        foreach (var ccuClient in ccuClients)
        {
            try
            {
                var completeDevice = await ccuClient.GetCompleteDeviceAsync(address).ConfigureAwait(false);

                return completeDevice;
            }
            catch (KeyNotFoundException)
            {
            }
        }

        throw new KeyNotFoundException($"Device with address '{address}' not found.");
    }

    private async Task<IEnumerable<T>> GetDataFromClientsAsync<T>(Func<ICcuClient, Task<IEnumerable<T>>> func)
    {
        var dataFromClients = new List<IEnumerable<T>>();

        foreach (var ccuClient in ccuClients)
        {
            var data = await func(ccuClient).ConfigureAwait(false);

            dataFromClients.Add(data);
        }

        return dataFromClients.SelectMany(x => x);
    }
}
