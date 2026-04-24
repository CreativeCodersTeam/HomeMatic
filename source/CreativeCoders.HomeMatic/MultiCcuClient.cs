using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic;

/// <inheritdoc />
/// <summary>
/// Aggregates several <see cref="T:CreativeCoders.HomeMatic.Core.ICcuClient">ICcuClient</see> instances into a single client that routes per-device
/// calls to the CCU that owns the device.
/// </summary>
/// <param name="ccuClients">The underlying CCU clients to dispatch calls to.</param>
/// <param name="routingTable">The routing table used to cache the mapping from device address to <see cref="T:CreativeCoders.HomeMatic.Core.ICcuClient">ICcuClient</see>.</param>
/// <remarks>
/// The first call to <see cref="M:CreativeCoders.HomeMatic.MultiCcuClient.GetDevicesAsync">GetDevicesAsync</see> or <see cref="M:CreativeCoders.HomeMatic.MultiCcuClient.GetCompleteDevicesAsync">GetCompleteDevicesAsync</see> populates the
/// <paramref name="routingTable" /> so that subsequent per-device calls can skip the full scan.
/// </remarks>
public class MultiCcuClient(IEnumerable<ICcuClient> ccuClients, ICcuRoutingTable routingTable)
    : IMultiCcuClient
{
    private readonly IReadOnlyList<ICcuClient> _ccuClients = Ensure.NotNull(ccuClients).ToList();

    private readonly ICcuRoutingTable _routingTable = Ensure.NotNull(routingTable);

    /// <inheritdoc />
    public async Task<IEnumerable<ICcuDevice>> GetDevicesAsync()
    {
        var results = await GetDataFromClientsAsync(x => x.GetDevicesAsync()).ConfigureAwait(false);

        // Populate the routing table so that subsequent per-device calls can skip the full scan.
        RegisterRoutes(results.SelectMany(pair => pair.Items.Select(item => (item.Uri.Address, pair.Client))));

        return results.SelectMany(pair => pair.Items);
    }

    /// <inheritdoc />
    public Task<ICcuDevice> GetDeviceAsync(string address)
    {
        Ensure.IsNotNullOrWhitespace(address);

        return InvokeWithRoutingAsync(address, (client, deviceAddress) => client.GetDeviceAsync(deviceAddress));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync()
    {
        var results = await GetDataFromClientsAsync(x => x.GetCompleteDevicesAsync()).ConfigureAwait(false);

        RegisterRoutes(results.SelectMany(pair =>
            pair.Items.Select(item => (item.DeviceData.Uri.Address, pair.Client))));

        return results.SelectMany(pair => pair.Items);
    }

    /// <inheritdoc />
    public Task<ICompleteCcuDevice> GetCompleteDeviceAsync(string address)
    {
        Ensure.IsNotNullOrWhitespace(address);

        return InvokeWithRoutingAsync(address,
            (client, deviceAddress) => client.GetCompleteDeviceAsync(deviceAddress));
    }

    // Generic helper that routes a per-device call through the routing table. Can be reused by future
    // per-device methods without duplicating the lookup/fallback logic.
    private async Task<TResult> InvokeWithRoutingAsync<TResult>(string address,
        Func<ICcuClient, string, Task<TResult>> func)
    {
        var deviceAddress = NormalizeAddress(address);

        if (_routingTable.TryGetClient(deviceAddress, out var cachedClient) && cachedClient is not null)
        {
            try
            {
                return await func(cachedClient, address).ConfigureAwait(false);
            }
            catch (KeyNotFoundException)
            {
                // Cached mapping is stale; drop it and fall back to probing the remaining clients.
                _routingTable.Invalidate(deviceAddress);
            }
        }

        foreach (var ccuClient in _ccuClients.Where(ccuClient => !ReferenceEquals(ccuClient, cachedClient)))
        {
            try
            {
                var result = await func(ccuClient, address).ConfigureAwait(false);

                _routingTable.Register(deviceAddress, ccuClient);

                return result;
            }
            catch (KeyNotFoundException)
            {
                // Device not found on this client; try the next one.
            }
        }

        throw new KeyNotFoundException($"Device with address '{address}' not found.");
    }

    private void RegisterRoutes(IEnumerable<(string Address, ICcuClient Client)> entries)
    {
        _routingTable.Register(entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Address))
            .Select(entry => new KeyValuePair<string, ICcuClient>(NormalizeAddress(entry.Address), entry.Client)));
    }

    // Device addresses may be suffixed with a channel index (e.g. "ABC0001234:1"). Routing is performed
    // on the device level, so we strip the channel part for lookups and registrations.
    private static string NormalizeAddress(string address)
    {
        var separatorIndex = address.IndexOf(':');

        return separatorIndex < 0 ? address : address[..separatorIndex];
    }

    private async Task<List<(ICcuClient Client, IEnumerable<T> Items)>> GetDataFromClientsAsync<T>(
        Func<ICcuClient, Task<IEnumerable<T>>> func)
    {
        var dataFromClients = new List<(ICcuClient Client, IEnumerable<T> Items)>();

        foreach (var ccuClient in _ccuClients)
        {
            var data = await func(ccuClient).ConfigureAwait(false);

            dataFromClients.Add((ccuClient, data));
        }

        return dataFromClients;
    }
}
