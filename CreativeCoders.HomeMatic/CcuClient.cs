using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Abstractions.Devices;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc;

namespace CreativeCoders.HomeMatic;

public class CcuClient(
    IHomeMaticJsonRpcClient jsonRpcClient,
    IDictionary<CcuDeviceKind, XmlRpcApiConnection> xmlRpcApis) : ICcuClient
{
    public async Task<IEnumerable<ICcuDevice>> GetDevicesAsync()
    {
        var allDevices = new List<CcuDevice>();

        foreach (var xmlRpcApiConnection in xmlRpcApis.Select(x => x.Value))
        {
            IReadOnlyCollection<DeviceDescription> devices =
                [..await xmlRpcApiConnection.Api.ListDevicesAsync().ConfigureAwait(false)];

            allDevices.AddRange(devices.Where(x => string.IsNullOrEmpty(x.Parent)).Select(x =>
                new CcuDeviceBuilder()
                    .FromDeviceDescription(x)
                    .WithApi(xmlRpcApiConnection.Api)
                    .WithUri(new CcuDeviceUri
                    {
                        CcuHost = xmlRpcApiConnection.Endpoint.BaseUrl.Host,
                        CcuName = xmlRpcApiConnection.CcuName,
                        Address = x.Address,
                        Kind = xmlRpcApiConnection.Endpoint.DeviceKind
                    })
                    .WithAllDevices(devices)
                    .Build()));
        }

        var jsonRpcDevices = await jsonRpcClient.ListAllDetailsAsync().ConfigureAwait(false);

        jsonRpcDevices.ForEach(x =>
        {
            var device =
                allDevices.FirstOrDefault(d => d.Uri.Address.Equals(x.Address, StringComparison.OrdinalIgnoreCase));

            if (device != null)
            {
                device.Name = x?.Name ?? string.Empty;
            }
        });

        return [..allDevices];
    }

    public async Task<ICcuDevice> GetDeviceAsync(string address)
    {
        return (await GetDevicesAsync().ConfigureAwait(false))
               .FirstOrDefault(device => device.Uri.Address.Equals(address, StringComparison.OrdinalIgnoreCase))
               ?? throw new KeyNotFoundException($"Device with address '{address}' not found.");
    }

    public Task<IEnumerable<ICompleteCcuDevice>> GetCompleteDevicesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ICompleteCcuDevice> GetCompleteDeviceAsync(string address)
    {
        throw new NotImplementedException();
    }
}
