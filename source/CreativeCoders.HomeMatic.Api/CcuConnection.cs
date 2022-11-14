using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.Api.Core;
using CreativeCoders.HomeMatic.Api.Core.Devices;
using CreativeCoders.HomeMatic.Api.Core.Parameters;
using CreativeCoders.HomeMatic.Api.Devices;
using CreativeCoders.HomeMatic.Api.Parameters;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api;

[PublicAPI]
public class CcuConnection : ICcuConnection
{
    public CcuConnection(IHomeMaticXmlRpcApi xmlRpcApi)
    {
        Ensure.IsNotNull(xmlRpcApi, nameof(xmlRpcApi));
            
        XmlRpcApi = xmlRpcApi;
    }
        
    public async Task<IEnumerable<ICcuDeviceInfo>> GetDeviceInfosAsync()
    {
        var deviceDescriptions = await XmlRpcApi.ListDevicesAsync().ConfigureAwait(false);

        return deviceDescriptions
            .Select(DeviceInfoCreator.Create)
            .ToArray();
    }

    public async Task<ICcuDeviceInfo> GetDeviceInfoAsync(string deviceAddress)
    {
        var deviceDescription = await XmlRpcApi.GetDeviceDescriptionAsync(deviceAddress).ConfigureAwait(false);
            
        return DeviceInfoCreator.Create(deviceDescription);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IEnumerable<ICcuDevice>> GetDevicesAsync()
    {
        var deviceInfos = await GetDeviceInfosAsync().ConfigureAwait(false);

        return deviceInfos
            .Where(dev => dev.IsDevice)
            .Select(x => CreateDevice(x, deviceInfos))
            .ToArray();
    }

    private ICcuDevice CreateDevice(ICcuDeviceInfo deviceInfo, IEnumerable<ICcuDeviceInfo> deviceInfos)
    {
        var device = new CcuDevice(deviceInfo, XmlRpcApi);

        device.AddChannels(
            deviceInfos
                .Where(x => x.IsChannel && x.Parent == deviceInfo.Address)
                .Select(x => new CcuDeviceChannel(device, x, XmlRpcApi)));

        return device;
    }

    public async Task<ICcuDevice> GetDeviceAsync(string deviceAddress)
    {
        var deviceInfo = await GetDeviceInfoAsync(deviceAddress).ConfigureAwait(false);
            
        var device = new CcuDevice(deviceInfo, XmlRpcApi);
            
        var channelInfos = new List<ICcuDeviceInfo>();

        foreach (var channelAddress in deviceInfo.Children)
        {
            var channelInfo = await GetDeviceInfoAsync(channelAddress).ConfigureAwait(false);
            channelInfos.Add(channelInfo);
        }

        device.AddChannels(channelInfos.Select(channelInfo => new CcuDeviceChannel(device, channelInfo, XmlRpcApi)));

        return device;
    }

    public Task<object> ReadValueAsync(string deviceAddress, string valueKey)
    {
        return XmlRpcApi.GetValueAsync(deviceAddress, valueKey);
    }

    public Task<T> ReadValueAsync<T>(string deviceAddress, string valueKey)
    {
        return XmlRpcApi.GetValueAsync<T>(deviceAddress, valueKey);
    }

    public Task WriteValueAsync(string deviceAddress, string valueKey, object value)
    {
        return XmlRpcApi.SetValueAsync(deviceAddress, valueKey, value);
    }

    public async Task<IEnumerable<IServiceMessage>> GetServiceMessagesAsync()
    {
        var messages = await XmlRpcApi.GetServiceMessagesAsync().ConfigureAwait(false);

        return messages
            .Select(x => x.ToArray())
            .Select(message => new ServiceMessage((string)message[0], (string)message[1], message[2]))
            .ToArray();
    }

    public Task RegisterEventServerAsync()
    {
        return XmlRpcApi.InitAsync(EventXmlRpcUrl, InterfaceId);
    }

    public Task UnregisterEventServerAsync()
    {
        return XmlRpcApi.InitAsync(EventXmlRpcUrl, string.Empty);
    }

    public async Task<CcuLogLevel> SetLogLevelAsync(CcuLogLevel logLevel)
    {
        return (CcuLogLevel) await XmlRpcApi.SetLogLevelAsync((int) logLevel).ConfigureAwait(false);
    }

    public async Task<CcuLogLevel> GetLogLevelAsync()
    {
        return (CcuLogLevel) await XmlRpcApi.GetLogLevelAsync().ConfigureAwait(false);
    }

    public Task<Dictionary<string, object>> ReadParamSetAsync(string deviceAddress, string paramSetKey)
    {
        return XmlRpcApi.GetParamSetAsync(deviceAddress, paramSetKey);
    }

    public async Task<Dictionary<string, ICcuParameterInfo>> GetParameterInfoAsync(string deviceAddress, string paramSetKey)
    {
        var parameterDescription =
            await XmlRpcApi.GetParameterDescriptionAsync(deviceAddress, paramSetKey)
                .ConfigureAwait(false);

        return parameterDescription.Keys.ToDictionary(key => key,
            key => (ICcuParameterInfo) ParameterInfoCreator.Create(parameterDescription[key]));
    }

    public async Task<IEnumerable<IBidcosInterfaceInfo>> GetBidcosInterfacesAsync()
    {
        var bidcosInterfaces = await XmlRpcApi
            .ListBidcosInterfacesAsync()
            .ConfigureAwait(false);

        return bidcosInterfaces
            .Select(BidcosInterfaceInfoCreator.Create);
    }

    public Task WriteParamsetAsync(string deviceAddress, string paramSetKey, IDictionary<string, object> paramSet)
    {
        return XmlRpcApi.PutParamsetAsync(deviceAddress, paramSetKey, paramSet);
    }

    public Task<bool> PingAsync(string callerId)
    {
        return XmlRpcApi.PingAsync(callerId);
    }

    public string InterfaceId { get; set; }
        
    public string EventXmlRpcUrl { get; set; }
        
    public IHomeMaticXmlRpcApi XmlRpcApi { get; }
}