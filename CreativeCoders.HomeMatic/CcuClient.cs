using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;

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
            var devices = await xmlRpcApiConnection.Api.ListDevicesAsync();

            allDevices.AddRange(devices.Select(x => new CcuDevice(xmlRpcApiConnection.Api)
            {
                Uri = new CcuDeviceUri
                {
                    Host = xmlRpcApiConnection.Endpoint.BaseUrl.Host,
                    Address = x.Address,
                    Kind = xmlRpcApiConnection.Endpoint.DeviceKind
                }
            }));
        }

        var jsonRpcDevices = await jsonRpcClient.ListAllDetailsAsync();

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
        return (await GetDevicesAsync())
               .FirstOrDefault(device => device.Uri.Address.Equals(address, StringComparison.OrdinalIgnoreCase))
               ?? throw new KeyNotFoundException($"Device with address '{address}' not found.");
    }
}
