using CreativeCoders.HomeMatic.Abstractions;

namespace CreativeCoders.HomeMatic;

public class MultiCcuClient(IEnumerable<ICcuClient> ccuClients) : IMultiCcuClient
{
    public async IAsyncEnumerable<ICcuDevice> GetDevicesAsync()
    {
        foreach (var ccuClient in ccuClients)
        {
            var devices = await ccuClient.GetDevicesAsync().ConfigureAwait(false);

            foreach (var ccuDevice in devices)
            {
                yield return ccuDevice;
            }
        }
    }
}
