using System.Collections.Generic;
using System.Threading.Tasks;
using CreativeCoders.HomeMatic.Api.Core.Devices;
using CreativeCoders.HomeMatic.Api.Core.Parameters;
using CreativeCoders.HomeMatic.Core;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Core;

[PublicAPI]
public interface ICcuConnection
{
    Task<IEnumerable<ICcuDeviceInfo>> GetDeviceInfosAsync();

    Task<ICcuDeviceInfo> GetDeviceInfoAsync(string deviceAddress);

    Task<IEnumerable<ICcuDevice>> GetDevicesAsync();

    Task<ICcuDevice> GetDeviceAsync(string deviceAddress);

    Task<object> ReadValueAsync(string deviceAddress, string valueKey);
        
    Task<T> ReadValueAsync<T>(string deviceAddress, string valueKey);

    Task WriteValueAsync(string deviceAddress, string valueKey, object value);
        
    Task<IEnumerable<IServiceMessage>> GetServiceMessagesAsync();

    Task RegisterEventServerAsync();

    Task UnregisterEventServerAsync();

    Task<CcuLogLevel> SetLogLevelAsync(CcuLogLevel logLevel);

    Task<CcuLogLevel> GetLogLevelAsync();

    Task<Dictionary<string, object>> ReadParamSetAsync(string deviceAddress, string paramSetKey);

    Task<Dictionary<string, ICcuParameterInfo>> GetParameterInfoAsync(string deviceAddress, string paramSetKey);

    Task<IEnumerable<IBidcosInterfaceInfo>> GetBidcosInterfacesAsync();

    Task WriteParamsetAsync(string deviceAddress, string paramSetKey, IDictionary<string, object> paramSet);

    Task<bool> PingAsync(string callerId);
        
    string InterfaceId { get; set; }
        
    string EventXmlRpcUrl { get; set; }
}